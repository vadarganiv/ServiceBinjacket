import { cookies } from 'next/headers';
import { redirect } from 'next/navigation';
import Link from 'next/link';
import { adminFetch } from '@/features/admin/lib/api';

interface ServiceItem {
  id: number;
  nameSq: string;
  nameEn: string | null;
  slugSq: string;
  categoryId: number;
  categoryName: string;
  isPublished: boolean;
}

export default async function AdminServicesPage({
  searchParams,
}: {
  searchParams: Promise<{ q?: string; isPublished?: string }>;
}) {
  const params = await searchParams;
  const cookieStore = await cookies();
  const token = cookieStore.get('sb_admin_token')?.value;
  if (!token) redirect('/admin/login');

  const qs = new URLSearchParams({ pageSize: '100' });
  if (params.q) qs.set('q', params.q);
  if (params.isPublished !== undefined) qs.set('isPublished', params.isPublished);

  const res = await adminFetch(`/admin/services?${qs}`, token);
  if (res.status === 401) redirect('/admin/login');

  const data = await res.json();
  const services: ServiceItem[] = data.items ?? [];
  const total: number = data.totalCount ?? 0;
  const publishFilter = params.isPublished;

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-semibold text-gray-900">Services</h1>
        <Link
          href="/admin/services/new"
          className="px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded-lg hover:bg-blue-700 transition-colors"
        >
          + Add service
        </Link>
      </div>

      <div className="flex flex-wrap gap-3 mb-4">
        <form method="get" className="flex gap-2">
          <input
            name="q"
            defaultValue={params.q ?? ''}
            placeholder="Search..."
            className="text-sm border border-gray-300 rounded-md px-3 py-1.5 focus:outline-none focus:ring-2 focus:ring-blue-500 w-48"
          />
          <button type="submit" className="px-3 py-1.5 bg-gray-700 text-white text-sm rounded-md hover:bg-gray-800">
            Search
          </button>
        </form>
        <div className="flex gap-2">
          {[
            { href: '/admin/services', label: 'All', value: undefined },
            { href: '/admin/services?isPublished=true', label: 'Published', value: 'true' },
            { href: '/admin/services?isPublished=false', label: 'Draft', value: 'false' },
          ].map(item => (
            <Link
              key={item.label}
              href={item.href}
              className={`px-3 py-1.5 rounded-md text-sm border ${publishFilter === item.value ? 'bg-blue-600 text-white border-blue-600' : 'bg-white text-gray-600 border-gray-300'}`}
            >
              {item.label}
            </Link>
          ))}
        </div>
        <span className="text-sm text-gray-500 self-center">{total} total</span>
      </div>

      {services.length === 0 ? (
        <div className="bg-white rounded-lg border p-8 text-center text-gray-400">No services found</div>
      ) : (
        <div className="bg-white rounded-lg border overflow-hidden">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">#</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Name</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Category</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                <th className="px-4 py-3" />
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {services.map(s => (
                <tr key={s.id} className="hover:bg-gray-50 transition-colors">
                  <td className="px-4 py-3 text-sm text-gray-500 font-mono">{s.id}</td>
                  <td className="px-4 py-3">
                    <p className="text-sm font-medium text-gray-900">{s.nameSq}</p>
                    {s.nameEn && <p className="text-xs text-gray-500">{s.nameEn}</p>}
                  </td>
                  <td className="px-4 py-3 text-sm text-gray-600">{s.categoryName}</td>
                  <td className="px-4 py-3">
                    <span className={`inline-flex px-2 py-0.5 rounded text-xs font-medium ${s.isPublished ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-600'}`}>
                      {s.isPublished ? 'Published' : 'Draft'}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-right">
                    <Link
                      href={`/admin/services/${s.id}`}
                      className="text-sm text-blue-600 hover:text-blue-800 font-medium"
                    >
                      Edit →
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
