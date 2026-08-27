import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { useQueryClient } from '@tanstack/react-query'
import { useGSAP } from '@gsap/react'
import gsap from 'gsap'
import { Bell, Bot, Command, LayoutDashboard, Settings, Sparkles, X } from 'lucide-react'
import { useEffect, useRef, useState } from 'react'
import { NavLink, Outlet, useLocation } from 'react-router-dom'
import { api, queryKeys } from '../api'

gsap.registerPlugin(useGSAP)
const navItems = [
  { to: '/', label: 'Overview', icon: LayoutDashboard, end: true },
  { to: '/assistant', label: 'Neural chat', icon: Bot },
  { to: '/reminders', label: 'Signals', icon: Bell },
  { to: '/settings', label: 'System', icon: Settings },
]

export function AppShell() {
  const queryClient = useQueryClient()
  const location = useLocation()
  const shellRef = useRef<HTMLDivElement>(null)
  const [toast, setToast] = useState<string | null>(null)
  const [time, setTime] = useState(() => new Date())

  useGSAP(() => {
    const mm = gsap.matchMedia()
    mm.add('(prefers-reduced-motion: no-preference)', () => {
      gsap.from('.sidebar-inner', { x: -24, autoAlpha: 0, duration: 0.75, ease: 'power3.out' })
      gsap.from('.ambient-orb', { scale: 0.7, autoAlpha: 0, duration: 1.8, stagger: 0.18, ease: 'power2.out' })
      gsap.to('.orb-a', { x: 40, y: 25, duration: 8, repeat: -1, yoyo: true, ease: 'sine.inOut' })
      gsap.to('.orb-b', { x: -30, y: -35, duration: 10, repeat: -1, yoyo: true, ease: 'sine.inOut' })
    })
    return () => mm.revert()
  }, { scope: shellRef })

  useGSAP(() => {
    gsap.fromTo('.content-stage', { autoAlpha: 0, y: 12 }, { autoAlpha: 1, y: 0, duration: 0.45, ease: 'power3.out' })
  }, { dependencies: [location.pathname], scope: shellRef, revertOnUpdate: true })

  useGSAP(() => {
    if (window.matchMedia('(pointer: coarse)').matches) return
    const dot = shellRef.current?.querySelector('.cursor-dot')
    if (!dot) return
    const xDot = gsap.quickTo(dot, 'x', { duration: 0.12, ease: 'power3.out' })
    const yDot = gsap.quickTo(dot, 'y', { duration: 0.12, ease: 'power3.out' })
    const move = (event: PointerEvent) => { xDot(event.clientX); yDot(event.clientY) }
    window.addEventListener('pointermove', move, { passive: true })
    return () => window.removeEventListener('pointermove', move)
  }, { scope: shellRef })

  useEffect(() => {
    const timer = window.setInterval(() => setTime(new Date()), 30_000)
    return () => window.clearInterval(timer)
  }, [])

  useEffect(() => {
    const connection = new HubConnectionBuilder().withUrl(`${api.base}/hubs/assistant`).withAutomaticReconnect().configureLogging(LogLevel.Warning).build()
    connection.on('notification', (payload: { title: string; message: string }) => {
      setToast(`${payload.title}: ${payload.message}`)
      window.setTimeout(() => setToast(null), 6000)
      void queryClient.invalidateQueries({ queryKey: queryKeys.reminders })
    })
    connection.on('dashboardChanged', () => {
      void queryClient.invalidateQueries({ queryKey: queryKeys.dashboard })
      void queryClient.invalidateQueries({ queryKey: queryKeys.goals })
    })
    void connection.start()
    return () => { void connection.stop() }
  }, [queryClient])

  return (
    <div className="shell" ref={shellRef}>
      <a className="skip-link" href="#main-content">Skip to content</a><div className="cursor-dot" aria-hidden="true" />
      <div className="noise" aria-hidden="true" /><div className="grid-plane" aria-hidden="true" />
      <div className="ambient-orb orb-a" aria-hidden="true" /><div className="ambient-orb orb-b" aria-hidden="true" />
      <aside className="sidebar"><div className="sidebar-inner">
        <div className="brand"><span className="brand-mark"><Sparkles size={20} /></span><div><p className="brand-name">NOVA</p><p className="brand-sub">Personal intelligence</p></div></div>
        <div className="system-status"><span className="status-dot" /><span>Core online</span><span className="status-code">v1.0</span></div>
        <nav aria-label="Primary navigation">{navItems.map(({ to, label, icon: Icon, end }) => <NavLink key={to} to={to} end={end} aria-label={label}><Icon size={19} strokeWidth={1.8} /><span>{label}</span><i /></NavLink>)}</nav>
        <div className="sidebar-footer"><div className="mini-orbit"><Command size={16} /><span /></div><div><p>Local intelligence</p><small>Encrypted workspace</small></div></div>
      </div></aside>
      <main className="content" id="main-content">
        <div className="topbar"><div className="breadcrumb"><span>NOVA</span><i>/</i><strong>{navItems.find(n => n.to === location.pathname)?.label ?? 'Goal protocol'}</strong></div><div className="topbar-meta"><span className="live-pill"><i /> LIVE</span><time>{time.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</time></div></div>
        <div className="content-stage"><Outlet /></div>
      </main>
      {toast ? <div className="toast" role="status"><span className="toast-icon"><Bell size={18} /></span><span>{toast}</span><button onClick={() => setToast(null)} aria-label="Dismiss notification"><X size={16} /></button></div> : null}
    </div>
  )
}






