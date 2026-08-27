import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import type { FormEvent } from 'react'
import { Link, useParams } from 'react-router-dom'
import { api, queryKeys } from '../api'

export function GoalDetailPage() {
  const { id = '' } = useParams()
  const queryClient = useQueryClient()
  const goalQuery = useQuery({ queryKey: queryKeys.goal(id), queryFn: () => api.getGoal(id), enabled: !!id })
  const tasksQuery = useQuery({ queryKey: queryKeys.tasks(id), queryFn: () => api.listTasks(id), enabled: !!id })
  const [taskTitle, setTaskTitle] = useState('')
  const [dueAt, setDueAt] = useState('')

  const createTask = useMutation({
    mutationFn: () =>
      api.createTask(id, {
        title: taskTitle,
        priority: 3,
        dueAt: dueAt ? new Date(dueAt).toISOString() : null,
        estimatedMinutes: 60,
      }),
    onSuccess: async () => {
      setTaskTitle('')
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.tasks(id) }),
        queryClient.invalidateQueries({ queryKey: queryKeys.goal(id) }),
        queryClient.invalidateQueries({ queryKey: queryKeys.dashboard }),
      ])
    },
  })

  const completeTask = useMutation({
    mutationFn: (taskId: string) => api.completeTask(taskId),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.tasks(id) }),
        queryClient.invalidateQueries({ queryKey: queryKeys.goal(id) }),
        queryClient.invalidateQueries({ queryKey: queryKeys.dashboard }),
      ])
    },
  })

  const recordProgress = useMutation({
    mutationFn: ({ taskId, percentage }: { taskId: string; percentage: number }) =>
      api.recordProgress(taskId, { percentage, note: 'Manual update' }),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: queryKeys.tasks(id) }),
        queryClient.invalidateQueries({ queryKey: queryKeys.goal(id) }),
        queryClient.invalidateQueries({ queryKey: queryKeys.dashboard }),
      ])
    },
  })

  function onCreate(event: FormEvent) {
    event.preventDefault()
    if (!taskTitle.trim()) return
    createTask.mutate()
  }

  if (goalQuery.isLoading || tasksQuery.isLoading) return <p className="muted">Loading goal…</p>
  if (!goalQuery.data) return <p className="error">Goal not found.</p>

  const goal = goalQuery.data
  const tasks = tasksQuery.data ?? []

  return (
    <div className="page">
      <Link className="back" to="/">
        ← Dashboard
      </Link>
      <header className="page-header">
        <div>
          <p className="eyebrow">{goal.trackStatus}</p>
          <h1>{goal.title}</h1>
          <p className="lede">{goal.description || goal.successCriteria || 'No description yet.'}</p>
        </div>
        <div className="stat">
          <strong>{goal.progressPercent}%</strong>
          <span>progress</span>
        </div>
      </header>

      <section className="panel">
        <h2>Add task</h2>
        <form className="row-form" onSubmit={onCreate}>
          <input value={taskTitle} onChange={(e) => setTaskTitle(e.target.value)} placeholder="Task title" required />
          <input type="datetime-local" value={dueAt} onChange={(e) => setDueAt(e.target.value)} />
          <button className="button primary" type="submit">
            Add task
          </button>
        </form>
      </section>

      <section className="panel">
        <h2>Tasks</h2>
        <div className="stack">
          {tasks.length === 0 ? <p className="muted">No tasks yet.</p> : null}
          {tasks.map((task) => (
            <div key={task.id} className="task-row">
              <div>
                <strong>{task.title}</strong>
                <span className="muted">
                  {task.status} · {task.progressPercent}%
                  {task.dueAt ? ` · due ${new Date(task.dueAt).toLocaleString()}` : ''}
                </span>
              </div>
              <div className="actions">
                <button className="button" type="button" onClick={() => recordProgress.mutate({ taskId: task.id, percentage: Math.min(100, task.progressPercent + 25) })}>
                  +25%
                </button>
                <button className="button primary" type="button" onClick={() => completeTask.mutate(task.id)} disabled={task.status === 'Done'}>
                  Complete
                </button>
              </div>
            </div>
          ))}
        </div>
      </section>
    </div>
  )
}
