import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { api, queryKeys } from '../api'

const prompts = [
  'What should I work on next?',
  'Which tasks are late?',
  'Give me a short motivational check-in.',
  'Summarize my progress this week.',
]

export function AssistantPage() {
  const queryClient = useQueryClient()
  const [conversationId, setConversationId] = useState<string | null>(null)
  const [input, setInput] = useState('')
  const [recording, setRecording] = useState(false)
  const [speaking, setSpeaking] = useState(false)
  const mediaRecorderRef = useRef<MediaRecorder | null>(null)
  const chunksRef = useRef<Blob[]>([])
  const audioRef = useRef<HTMLAudioElement | null>(null)

  useEffect(() => {
    void api.createConversation('Web chat').then((c) => setConversationId(c.id))
  }, [])

  const messagesQuery = useQuery({
    queryKey: queryKeys.messages(conversationId ?? ''),
    queryFn: () => api.getMessages(conversationId!),
    enabled: !!conversationId,
  })

  const send = useMutation({
    mutationFn: async (content: string) => {
      if (!conversationId) throw new Error('Conversation not ready')
      return api.sendMessage(conversationId, content)
    },
    onSuccess: async (result) => {
      setInput('')
      await queryClient.invalidateQueries({ queryKey: queryKeys.messages(conversationId ?? '') })
      await queryClient.invalidateQueries({ queryKey: queryKeys.dashboard })
      if (result.reply) {
        try {
          const blob = await api.synthesize(result.reply)
          const url = URL.createObjectURL(blob)
          if (audioRef.current) {
            audioRef.current.pause()
          }
          const audio = new Audio(url)
          audioRef.current = audio
          setSpeaking(true)
          audio.onended = () => setSpeaking(false)
          await audio.play()
        } catch {
          setSpeaking(false)
        }
      }
    },
  })

  function onSubmit(event: FormEvent) {
    event.preventDefault()
    if (!input.trim() || send.isPending) return
    send.mutate(input.trim())
  }

  async function toggleRecord() {
    if (recording) {
      mediaRecorderRef.current?.stop()
      setRecording(false)
      return
    }

    const stream = await navigator.mediaDevices.getUserMedia({ audio: true })
    const recorder = new MediaRecorder(stream)
    chunksRef.current = []
    recorder.ondataavailable = (event) => {
      if (event.data.size > 0) chunksRef.current.push(event.data)
    }
    recorder.onstop = async () => {
      stream.getTracks().forEach((t) => t.stop())
      const blob = new Blob(chunksRef.current, { type: 'audio/webm' })
      try {
        const result = await api.transcribe(blob)
        setInput(result.text)
      } catch (error) {
        setInput(`(transcription failed: ${error instanceof Error ? error.message : 'unknown error'})`)
      }
    }
    mediaRecorderRef.current = recorder
    recorder.start()
    setRecording(true)
  }

  function stopAudio() {
    audioRef.current?.pause()
    setSpeaking(false)
  }

  const messages = messagesQuery.data ?? []

  return (
    <div className="page assistant-page">
      <header className="page-header">
        <div>
          <p className="eyebrow">Assistant</p>
          <h1>Talk through your goals</h1>
          <p className="lede">The assistant reads and updates your PostgreSQL data through tools.</p>
        </div>
      </header>

      <div className="prompt-row">
        {prompts.map((prompt) => (
          <button key={prompt} className="chip" type="button" onClick={() => setInput(prompt)}>
            {prompt}
          </button>
        ))}
      </div>

      <section className="chat panel">
        <div className="messages">
          {messages
            .filter((m) => m.role === 'User' || m.role === 'Assistant')
            .map((message) => (
              <div key={message.id} className={`bubble ${message.role.toLowerCase()}`}>
                <span className="role">{message.role}</span>
                <p>{message.content}</p>
              </div>
            ))}
          {send.isPending ? <p className="muted">Thinking and checking tools…</p> : null}
        </div>

        <form className="composer" onSubmit={onSubmit}>
          <textarea
            value={input}
            onChange={(e) => setInput(e.target.value)}
            placeholder="Ask what to work on next…"
            rows={3}
          />
          <div className="composer-actions">
            <button className="button" type="button" onClick={() => void toggleRecord()}>
              {recording ? 'Stop mic' : 'Push to talk'}
            </button>
            <button className="button" type="button" onClick={stopAudio} disabled={!speaking}>
              Stop audio
            </button>
            <button className="button primary" type="submit" disabled={!conversationId || send.isPending}>
              Send
            </button>
          </div>
        </form>
      </section>
    </div>
  )
}
