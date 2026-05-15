import { cookies } from 'next/headers';
import { redirect, notFound } from 'next/navigation';
import Link from 'next/link';
import { adminFetch } from '@/features/admin/lib/api';
import ServiceForm from '@/features/admin/services/ServiceForm';
import ServicePublishToggle from '@/features/admin/services/ServicePublishToggle';

export default async function AdminServiceEditPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  const cookieStore = await cookies();
  const token = cookieStore.get('sb_admin_token')?.value;
  if (!token) redirect('/admin/login');

  const [serviceRes, catRes] = await Promise.all([
    adminFetch(`/admin/services/${id}`, token),
    adminFetch('/admin/services/categories', token),
  ]);

  if (serviceRes.status === 401) redirect('/admin/login');
  if (serviceRes.status === 404) notFound();

  const service = await serviceRes.json();
  const categories = await catRes.json();

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <Link href="/admin/services" className="text-sm text-gray-500 hover:text-gray-700">
          ← Back to services
        </Link>
        <ServicePublishToggle serviceId={service.id} isPublished={service.isPublished} />
      </div>

      <div className="flex items-center gap-3 mb-6">
        <h1 className="text-2xl font-semibold text-gray-900">Edit: {service.nameSq}</h1>
        <span className={`text-xs px-2 py-0.5 rounded font-medium ${service.isPublished ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-600'}`}>
          {service.isPublished ? 'Published' : 'Draft'}
        </span>
      </div>

      <ServiceForm categories={categories} initialData={{ ...service }} mode="edit" />
    </div>
  );
}
