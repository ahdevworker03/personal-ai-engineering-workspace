import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import type { FormEvent } from 'react'
import { api, queryKeys } from '../api'

export function RemindersPage() {
  const queryClient = useQueryClient()
  const { data = [], isLoading } = useQuery({ queryKey: queryKeys.reminders, queryFn: api.listReminders })
  const [title, setTitle] = useState('Focus check')
  const [message, setMessage] = useState('Return to your current priority task.')
  const [scheduledAt, setScheduledAt] = useState('')

  const create = useMutation({
    mutationFn: () =>
      api.createReminder({
        title,
        message,
        scheduledAt: new Date(scheduledAt).toISOString(),
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: queryKeys.reminders })
      await queryClient.invalidateQueries({ queryKey: queryKeys.dashboard })
    },
  })

  const snooze = useMutation({
    mutationFn: (id: string) =>
      api.snoozeReminder(id, new Date(Date.now() + 60 * 60 * 1000).toISOString()),
    onSuccess: async () => queryClient.invalidateQueries({ queryKey: queryKeys.reminders }),
  })

  const cancel = useMutation({
    mutationFn: (id: string) => api.cancelReminder(id),
    onSuccess: async () => queryClient.invalidateQueries({ queryKey: queryKeys.reminders }),
  })

  function onCreate(event: FormEvent) {
    event.preventDefault()
    if (!scheduledAt) return
    create.mutate()
  }

  return (
    <div className="page">
      <header className="page-header">
        <div>
          <p className="eyebrow">Reminders</p>
          <h1>Stay on schedule</h1>
          <p className="lede">Hangfire keeps reminders alive across API restarts.</p>
        </div>
      </header>

      <section className="panel">
        <h2>Create reminder</h2>
        <form className="stack-form" onSubmit={onCreate}>
          <input value={title} onChange={(e) => setTitle(e.target.value)} required />
          <textarea value={message} onChange={(e) => setMessage(e.target.value)} rows={3} required />
          <input type="datetime-local" value={scheduledAt} onChange={(e) => setScheduledAt(e.target.value)} required />
          <button className="button primary" type="submit">
            Schedule
          </button>
        </form>
      </section>

      <section className="panel">
        <h2>Upcoming</h2>
        {isLoading ? <p className="muted">Loading…</p> : null}
        <div className="stack">
          {data.map((reminder) => (
            <div key={reminder.id} className="task-row">
              <div>
                <strong>{reminder.title}</strong>
                <span className="muted">
                  {reminder.status} · {new Date(reminder.scheduledAt).toLocaleString()}
                </span>
                <p>{reminder.message}</p>
              </div>
              <div className="actions">
                <button className="button" type="button" onClick={() => snooze.mutate(reminder.id)}>
                  Snooze 1h
                </button>
                <button className="button" type="button" onClick={() => cancel.mutate(reminder.id)}>
                  Cancel
                </button>
              </div>
            </div>
          ))}
        </div>
      </section>
    </div>
  )
}
