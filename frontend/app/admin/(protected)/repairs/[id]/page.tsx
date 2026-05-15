import { cookies } from 'next/headers';
import { redirect, notFound } from 'next/navigation';
import Link from 'next/link';
import { adminFetch } from '@/features/admin/lib/api';
import RepairDetailClient from '@/features/admin/repairs/RepairDetailClient';

export default async function AdminRepairDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  const cookieStore = await cookies();
  const token = cookieStore.get('sb_admin_token')?.value;
  if (!token) redirect('/admin/login');

  const res = await adminFetch(`/admin/repair-requests/${id}`, token);
  if (res.status === 401) redirect('/admin/login');
  if (res.status === 404) notFound();

  const repair = await res.json();

  return (
    <div>
      <Link href="/admin/repairs" className="text-sm text-gray-500 hover:text-gray-700 mb-4 inline-block">
        ← Back to repairs
      </Link>
      <RepairDetailClient repair={repair} />
    </div>
  );
}
