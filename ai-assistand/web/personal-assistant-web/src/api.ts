const API_BASE = import.meta.env.VITE_API_BASE ?? 'http://localhost:5080'

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${API_BASE}${path}`, {
    headers: {
      'Content-Type': 'application/json',
      ...(init?.headers ?? {}),
    },
    ...init,
  })

  if (!response.ok) {
    const body = await response.text()
    throw new Error(body || response.statusText)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return response.json() as Promise<T>
}

export type Goal = {
  id: string
  title: string
  description?: string | null
  category?: string | null
  horizon: string
  status: string
  priority: number
  startDate?: string | null
  targetDate?: string | null
  progressPercent: number
  progressManuallyOverridden: boolean
  successCriteria?: string | null
  trackStatus: string
  createdAt: string
  updatedAt: string
  completedAt?: string | null
}

export type Task = {
  id: string
  goalId: string
  parentTaskId?: string | null
  title: string
  description?: string | null
  status: string
  priority: number
  dueAt?: string | null
  estimatedMinutes?: number | null
  actualMinutes?: number | null
  progressPercent: number
  sortOrder: number
  createdAt: string
  updatedAt: string
  completedAt?: string | null
}

export type Reminder = {
  id: string
  goalId?: string | null
  taskId?: string | null
  title: string
  message: string
  scheduledAt: string
  recurrenceRule?: string | null
  status: string
  createdAt: string
}

export type Dashboard = {
  greeting: string
  today: string
  activeGoals: Goal[]
  tasksDueToday: Task[]
  overdueTasks: Task[]
  nextReminder?: Reminder | null
  recentProgressSummary: string
}

export type ConversationMessage = {
  id: string
  conversationId: string
  role: string
  content: string
  toolName?: string | null
  createdAt: string
}

export type Profile = {
  id: string
  displayName: string
  timeZone: string
  preferredLanguage: string
  preferredTone: string
  dailyCheckInTime?: string | null
}

export const api = {
  base: API_BASE,
  getDashboard: () => request<Dashboard>('/api/dashboard'),
  listGoals: () => request<Goal[]>('/api/goals'),
  getGoal: (id: string) => request<Goal>(`/api/goals/${id}`),
  createGoal: (body: Record<string, unknown>) =>
    request<Goal>('/api/goals', { method: 'POST', body: JSON.stringify(body) }),
  updateGoal: (id: string, body: Record<string, unknown>) =>
    request<Goal>(`/api/goals/${id}`, { method: 'PATCH', body: JSON.stringify(body) }),
  archiveGoal: (id: string) =>
    request<void>(`/api/goals/${id}/archive`, { method: 'POST' }),
  listTasks: (goalId: string) => request<Task[]>(`/api/goals/${goalId}/tasks`),
  createTask: (goalId: string, body: Record<string, unknown>) =>
    request<Task>(`/api/goals/${goalId}/tasks`, { method: 'POST', body: JSON.stringify(body) }),
  updateTask: (id: string, body: Record<string, unknown>) =>
    request<Task>(`/api/tasks/${id}`, { method: 'PATCH', body: JSON.stringify(body) }),
  completeTask: (id: string) =>
    request<Task>(`/api/tasks/${id}/complete`, { method: 'POST' }),
  recordProgress: (id: string, body: Record<string, unknown>) =>
    request(`/api/tasks/${id}/progress`, { method: 'POST', body: JSON.stringify(body) }),
  listReminders: () => request<Reminder[]>('/api/reminders'),
  createReminder: (body: Record<string, unknown>) =>
    request<Reminder>('/api/reminders', { method: 'POST', body: JSON.stringify(body) }),
  snoozeReminder: (id: string, newScheduledAt: string) =>
    request<Reminder>(`/api/reminders/${id}/snooze`, {
      method: 'POST',
      body: JSON.stringify({ newScheduledAt }),
    }),
  cancelReminder: (id: string) =>
    request<void>(`/api/reminders/${id}/cancel`, { method: 'POST' }),
  createConversation: (title?: string) =>
    request<{ id: string }>('/api/conversations', {
      method: 'POST',
      body: JSON.stringify({ title }),
    }),
  getMessages: (id: string) => request<ConversationMessage[]>(`/api/conversations/${id}/messages`),
  sendMessage: (id: string, content: string) =>
    request<{ reply: string; toolsUsed: string[] }>(`/api/conversations/${id}/messages`, {
      method: 'POST',
      body: JSON.stringify({ content }),
    }),
  getProfile: () => request<Profile>('/api/profile'),
  updateProfile: (body: Record<string, unknown>) =>
    request<Profile>('/api/profile', { method: 'PATCH', body: JSON.stringify(body) }),
  weeklyReview: () => request<{ summary: string }>('/api/reviews/weekly'),
  synthesize: async (text: string) => {
    const response = await fetch(`${API_BASE}/api/voice/synthesize`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ text }),
    })
    if (!response.ok) throw new Error(await response.text())
    return response.blob()
  },
  transcribe: async (blob: Blob) => {
    const form = new FormData()
    form.append('audio', blob, 'recording.webm')
    const response = await fetch(`${API_BASE}/api/voice/transcribe`, {
      method: 'POST',
      body: form,
    })
    if (!response.ok) throw new Error(await response.text())
    return response.json() as Promise<{ text: string }>
  },
}

export const queryKeys = {
  dashboard: ['dashboard'] as const,
  goals: ['goals'] as const,
  goal: (id: string) => ['goals', id] as const,
  tasks: (goalId: string) => ['tasks', goalId] as const,
  reminders: ['reminders'] as const,
  messages: (id: string) => ['messages', id] as const,
  profile: ['profile'] as const,
}
