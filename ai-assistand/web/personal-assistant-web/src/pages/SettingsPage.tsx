import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { api, queryKeys } from '../api'

export function SettingsPage() {
  const queryClient = useQueryClient()
  const { data } = useQuery({ queryKey: queryKeys.profile, queryFn: api.getProfile })
  const [displayName, setDisplayName] = useState('')
  const [timeZone, setTimeZone] = useState('UTC')
  const [preferredTone, setPreferredTone] = useState('supportive and direct')
  const [weekly, setWeekly] = useState('')

  useEffect(() => {
    if (!data) return
    setDisplayName(data.displayName)
    setTimeZone(data.timeZone)
    setPreferredTone(data.preferredTone)
  }, [data])

  const save = useMutation({
    mutationFn: () => api.updateProfile({ displayName, timeZone, preferredTone }),
    onSuccess: async () => queryClient.invalidateQueries({ queryKey: queryKeys.profile }),
  })

  async function loadWeekly() {
    const result = await api.weeklyReview()
    setWeekly(result.summary)
  }

  function onSubmit(event: FormEvent) {
    event.preventDefault()
    save.mutate()
  }

  return (
    <div className="page">
      <header className="page-header">
        <div>
          <p className="eyebrow">Settings</p>
          <h1>Profile & keys</h1>
          <p className="lede">API keys stay in backend configuration, not the browser.</p>
        </div>
      </header>

      <section className="panel">
        <h2>User profile</h2>
        <form className="stack-form" onSubmit={onSubmit}>
          <label>
            Display name
            <input value={displayName} onChange={(e) => setDisplayName(e.target.value)} />
          </label>
          <label>
            Time zone
            <input value={timeZone} onChange={(e) => setTimeZone(e.target.value)} />
          </label>
          <label>
            Preferred tone
            <input value={preferredTone} onChange={(e) => setPreferredTone(e.target.value)} />
          </label>
          <button className="button primary" type="submit">
            Save profile
          </button>
        </form>
      </section>

      <section className="panel">
        <h2>Provider configuration</h2>
        <p className="muted">
          Set `DeepSeek__ApiKey` and `FishAudio__ApiKey` in environment variables or
          `src/PersonalAssistant.Api/appsettings.json`. Keys are never returned by the API.
        </p>
        <ul className="list">
          <li>API base: {api.base || window.location.origin}</li>
          <li>Swagger: {(api.base || window.location.origin)}/swagger</li>
          <li>Hangfire: {(api.base || window.location.origin)}/hangfire</li>
          <li>Health: {(api.base || window.location.origin)}/health</li>
        </ul>
      </section>

      <section className="panel">
        <h2>Weekly review</h2>
        <button className="button" type="button" onClick={() => void loadWeekly()}>
          Generate weekly summary
        </button>
        {weekly ? <p className="review">{weekly}</p> : null}
      </section>
    </div>
  )
}
