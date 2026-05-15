import { cookies } from 'next/headers';
import { redirect } from 'next/navigation';
import Link from 'next/link';
import { adminFetch } from '@/features/admin/lib/api';

async function fetchCount(path: string, token: string): Promise<number> {
  try {
    const res = await adminFetch(path, token);
    if (!res.ok) return 0;
    const data = await res.json();
    return data.totalCount ?? 0;
  } catch {
    return 0;
  }
}

export default async function AdminDashboardPage() {
  const cookieStore = await cookies();
  const token = cookieStore.get('sb_admin_token')?.value;
  if (!token) redirect('/admin/login');

  const [orders, repairs, products, services] = await Promise.all([
    fetchCount('/admin/orders?pageSize=1', token),
    fetchCount('/admin/repair-requests?pageSize=1', token),
    fetchCount('/admin/products?pageSize=1', token),
    fetchCount('/admin/services?pageSize=1', token),
  ]);

  const cards = [
    { label: 'Orders', count: orders, href: '/admin/orders', color: 'text-blue-600' },
    { label: 'Repair Requests', count: repairs, href: '/admin/repairs', color: 'text-orange-600' },
    { label: 'Products', count: products, href: '/admin/products', color: 'text-green-600' },
    { label: 'Services', count: services, href: '/admin/services', color: 'text-purple-600' },
  ];

  return (
    <div>
      <h1 className="text-2xl font-semibold text-gray-900 mb-6">Dashboard</h1>
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        {cards.map(card => (
          <Link key={card.label} href={card.href} className="bg-white rounded-lg border p-5 hover:shadow-sm transition-shadow">
            <p className="text-sm text-gray-500">{card.label}</p>
            <p className={`mt-2 text-3xl font-bold ${card.color}`}>{card.count}</p>
          </Link>
        ))}
      </div>
    </div>
  );
}
