import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useGSAP } from '@gsap/react'
import gsap from 'gsap'
import { ArrowUpRight, Bot, CalendarDays, ChevronRight, CircleDot, Clock3, Crosshair, Plus, Radio, Sparkles, Target, Zap } from 'lucide-react'
import { lazy, Suspense, useRef, useState } from 'react'
import type { FormEvent, ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { api, queryKeys } from '../api'
const NeuralScene = lazy(() => import('../components/NeuralScene').then(module => ({ default: module.NeuralScene })))

export function DashboardPage() {
  const queryClient = useQueryClient()
  const pageRef = useRef<HTMLDivElement>(null)
  const { data, isLoading, error } = useQuery({ queryKey: queryKeys.dashboard, queryFn: api.getDashboard })
  const [title, setTitle] = useState('')
  const [targetDate, setTargetDate] = useState('')
  const [showCreate, setShowCreate] = useState(false)

  useGSAP(() => {
    if (!data) return
    const mm = gsap.matchMedia()
    mm.add('(prefers-reduced-motion: no-preference)', () => {
      const tl = gsap.timeline({ defaults: { ease: 'power3.out' } })
      tl.from('.hero-copy > *', { y: 24, autoAlpha: 0, duration: 0.7, stagger: 0.08 })
        .from('.core-shell', { scale: 0.78, autoAlpha: 0, duration: 1.1, ease: 'expo.out' }, '<0.1')
        .from('.metric-card', { y: 18, autoAlpha: 0, duration: 0.55, stagger: 0.07 }, '-=0.55')
        .from('.dashboard-grid .panel', { y: 20, autoAlpha: 0, duration: 0.6, stagger: 0.09 }, '-=0.3')
      gsap.from('.progress > div', { scaleX: 0, transformOrigin: 'left center', duration: 1.1, stagger: 0.1, ease: 'power3.out' })
    })
    return () => mm.revert()
  }, { dependencies: [data], scope: pageRef, revertOnUpdate: true })

  const createGoal = useMutation({
    mutationFn: () => api.createGoal({ title, description: '', horizon: 'NearTerm', priority: 3, targetDate: targetDate || null, successCriteria: 'Ship a working local MVP' }),
    onSuccess: async () => {
      setTitle(''); setShowCreate(false)
      await Promise.all([queryClient.invalidateQueries({ queryKey: queryKeys.dashboard }), queryClient.invalidateQueries({ queryKey: queryKeys.goals })])
    },
  })

  function onCreate(event: FormEvent) { event.preventDefault(); if (title.trim()) createGoal.mutate() }
  if (isLoading) return <div className="boot-screen"><div className="boot-ring" /><p>Synchronizing personal intelligence...</p></div>
  if (error || !data) return <p className="error">Neural link unavailable. Is the API running?</p>

  const taskCount = data.tasksDueToday.length + data.overdueTasks.length
  const avgProgress = data.activeGoals.length ? Math.round(data.activeGoals.reduce((sum, goal) => sum + goal.progressPercent, 0) / data.activeGoals.length) : 0

  return (
    <div className="page command-page" ref={pageRef}>
      <section className="command-hero">
        <div className="hero-copy">
          <p className="eyebrow"><Radio size={11} /> Intelligence synchronized · {data.today}</p>
          <h1>{data.greeting}<br /><span>Let’s bend the day.</span></h1>
          <p className="lede">{data.recentProgressSummary || 'Your personal operating system is online. Pick a signal and move it forward.'}</p>
          <div className="hero-actions">
            <Link className="button primary launch-button" to="/assistant"><Bot size={16} /> Enter neural chat <ArrowUpRight size={15} /></Link>
            <button className="button ghost-button" onClick={() => setShowCreate(value => !value)}><Plus size={16} /> New objective</button>
          </div>
        </div>
        <div className="core-shell">
          <Suspense fallback={<div className="core-loader" />}><NeuralScene /></Suspense>
          <div className="core-hud hud-top"><span>COGNITIVE CORE</span><b>ONLINE</b></div>
          <div className="core-hud hud-bottom"><span>SYNC RATE</span><b>99.8%</b></div>
          <div className="core-reticle"><i /><i /><i /><i /></div>
        </div>
      </section>

      {showCreate ? <section className="panel create-drawer">
        <div><p className="panel-kicker">New protocol</p><h2>Initialize an objective</h2></div>
        <form className="row-form" onSubmit={onCreate}>
          <label className="sr-only" htmlFor="goal-title">Goal title</label>
          <input id="goal-title" value={title} onChange={e => setTitle(e.target.value)} placeholder="Name the next impossible thing..." autoFocus required />
          <label className="sr-only" htmlFor="goal-date">Target date</label>
          <input id="goal-date" type="date" value={targetDate} onChange={e => setTargetDate(e.target.value)} />
          <button className="button primary" type="submit" disabled={createGoal.isPending}>{createGoal.isPending ? 'Initializing...' : 'Initialize'}</button>
        </form>
      </section> : null}

      <section className="metrics-ribbon">
        <Metric icon={<Target />} value={data.activeGoals.length} label="Active protocols" signal="+ synchronized" />
        <Metric icon={<Crosshair />} value={taskCount} label="Focus signals" signal={data.overdueTasks.length + ' require attention'} hot={data.overdueTasks.length > 0} />
        <Metric icon={<Zap />} value={avgProgress + '%'} label="Momentum index" signal="live calculation" />
        <Metric icon={<Clock3 />} value={data.nextReminder ? new Date(data.nextReminder.scheduledAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : '—'} label="Next transmission" signal={data.nextReminder?.title ?? 'clear horizon'} />
      </section>

      <section className="dashboard-grid">
        <div className="panel objectives-panel">
          <div className="panel-heading"><div><p className="panel-kicker">Trajectory matrix</p><h2>Active objectives</h2></div><span className="count-badge">{data.activeGoals.length.toString().padStart(2, '0')}</span></div>
          <div className="objective-stack">
            {data.activeGoals.length === 0 ? <EmptyState text="No objectives initialized yet." /> : data.activeGoals.map((goal, index) => (
              <Link key={goal.id} className="objective-card" to={'/goals/' + goal.id}>
                <span className="objective-index">{String(index + 1).padStart(2, '0')}</span>
                <div className="objective-main"><div className="objective-title"><strong>{goal.title}</strong><span className={'track-pill ' + goal.trackStatus.toLowerCase().replaceAll(' ', '-')}><i />{goal.trackStatus}</span></div>
                  <div className="progress"><div style={{ width: goal.progressPercent + '%' }} /></div>
                  <div className="objective-meta"><span>{goal.targetDate ? 'Target ' + new Date(goal.targetDate).toLocaleDateString() : 'Open horizon'}</span><b>{goal.progressPercent}%</b></div>
                </div><ChevronRight size={17} className="objective-arrow" />
              </Link>
            ))}
          </div>
        </div>

        <div className="panel signal-panel">
          <div className="panel-heading"><div><p className="panel-kicker">Priority uplink</p><h2>Focus signals</h2></div><span className="signal-radar"><i /></span></div>
          <h3><CircleDot size={12} /> Due today</h3>
          <TaskList items={data.tasksDueToday} empty="The channel is clear." />
          <h3 className="danger-heading"><Zap size={12} /> Attention required</h3>
          <TaskList items={data.overdueTasks} empty="No overdue signals." danger />
          <div className="next-transmission">
            <span className="transmission-icon"><CalendarDays size={17} /></span>
            <div><small>Next transmission</small><strong>{data.nextReminder?.title ?? 'No signal scheduled'}</strong></div>
            <time>{data.nextReminder ? new Date(data.nextReminder.scheduledAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : '—'}</time>
          </div>
        </div>
      </section>
    </div>
  )
}

function Metric({ icon, value, label, signal, hot = false }: { icon: ReactNode; value: string | number; label: string; signal: string; hot?: boolean }) {
  return <article className={'metric-card ' + (hot ? 'hot' : '')}><div className="metric-icon">{icon}</div><div><strong>{value}</strong><span>{label}</span><small>{signal}</small></div><Sparkles size={13} className="metric-spark" /></article>
}
function TaskList({ items, empty, danger = false }: { items: { id: string; title: string; progressPercent: number }[]; empty: string; danger?: boolean }) {
  if (!items.length) return <p className="empty-signal">{empty}</p>
  return <div className="focus-list">{items.map(item => <div key={item.id} className={'focus-item ' + (danger ? 'danger' : '')}><span className="focus-node" /><div><strong>{item.title}</strong><small>{item.progressPercent}% resolved</small></div><b>{item.progressPercent}%</b></div>)}</div>
}
function EmptyState({ text }: { text: string }) { return <div className="empty-state"><span className="empty-orbit"><Target size={22} /></span><p>{text}</p></div> }


