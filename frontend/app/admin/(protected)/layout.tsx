import { cookies } from 'next/headers';
import { redirect } from 'next/navigation';
import AdminShell from '@/features/admin/AdminShell';

export default async function ProtectedAdminLayout({ children }: { children: React.ReactNode }) {
  const cookieStore = await cookies();
  const token = cookieStore.get('sb_admin_token');

  if (!token) {
    redirect('/admin/login');
  }

  const apiUrl = process.env.API_URL ?? 'http://localhost:5000';
  const res = await fetch(`${apiUrl}/api/v1/auth/me`, {
    headers: { Cookie: `sb_admin_token=${token.value}` },
    cache: 'no-store',
  });

  if (!res.ok) {
    redirect('/admin/login');
  }

  const admin = await res.json();

  return <AdminShell adminEmail={admin.email} adminName={admin.displayName}>{children}</AdminShell>;
}
