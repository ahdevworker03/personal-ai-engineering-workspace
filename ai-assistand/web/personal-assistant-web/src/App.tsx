import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'
import { AppShell } from './layout/AppShell'
import { AssistantPage } from './pages/AssistantPage'
import { DashboardPage } from './pages/DashboardPage'
import { GoalDetailPage } from './pages/GoalDetailPage'
import { RemindersPage } from './pages/RemindersPage'
import { SettingsPage } from './pages/SettingsPage'
import './styles.css'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      retry: 1,
    },
  },
})

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>
          <Route element={<AppShell />}>
            <Route index element={<DashboardPage />} />
            <Route path="goals/:id" element={<GoalDetailPage />} />
            <Route path="assistant" element={<AssistantPage />} />
            <Route path="reminders" element={<RemindersPage />} />
            <Route path="settings" element={<SettingsPage />} />
            <Route path="*" element={<Navigate to="/" replace />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  )
}
