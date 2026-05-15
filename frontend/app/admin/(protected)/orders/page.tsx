import { cookies } from 'next/headers';
import { redirect } from 'next/navigation';
import Link from 'next/link';
import { adminFetch } from '@/features/admin/lib/api';
import StatusBadge from '@/features/admin/components/StatusBadge';

const ORDER_STATUSES = [
  'New', 'Confirmed', 'Preparing', 'OutForDelivery',
  'SentByPost', 'Delivered', 'Completed', 'Cancelled',
];

interface OrderItem {
  id: number;
  customerName: string;
  customerPhone: string;
  subtotal: number;
  currency: string;
  status: string;
  itemCount: number;
  createdAt: string;
}

export default async function AdminOrdersPage({
  searchParams,
}: {
  searchParams: Promise<{ status?: string; page?: string }>;
}) {
  const params = await searchParams;
  const cookieStore = await cookies();
  const token = cookieStore.get('sb_admin_token')?.value;
  if (!token) redirect('/admin/login');

  const page = parseInt(params.page ?? '1', 10);
  const status = params.status ?? '';
  const qs = new URLSearchParams({ page: String(page), pageSize: '20' });
  if (status) qs.set('status', status);

  const res = await adminFetch(`/admin/orders?${qs}`, token);
  if (res.status === 401) redirect('/admin/login');

  const data = await res.json();
  const orders: OrderItem[] = data.items ?? [];
  const total: number = data.totalCount ?? 0;
  const pageCount = Math.ceil(total / 20);

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-semibold text-gray-900">Orders</h1>
        <span className="text-sm text-gray-500">{total} total</span>
      </div>

      {/* Status filter */}
      <div className="flex flex-wrap gap-2 mb-4">
        <Link
          href="/admin/orders"
          className={`px-3 py-1 rounded-full text-sm border transition-colors ${!status ? 'bg-blue-600 text-white border-blue-600' : 'bg-white text-gray-600 border-gray-300 hover:border-gray-400'}`}
        >
          All
        </Link>
        {ORDER_STATUSES.map(s => (
          <Link
            key={s}
            href={`/admin/orders?status=${s}`}
            className={`px-3 py-1 rounded-full text-sm border transition-colors ${status === s ? 'bg-blue-600 text-white border-blue-600' : 'bg-white text-gray-600 border-gray-300 hover:border-gray-400'}`}
          >
            {s}
          </Link>
        ))}
      </div>

      {orders.length === 0 ? (
        <div className="bg-white rounded-lg border p-8 text-center text-gray-400">No orders found</div>
      ) : (
        <div className="bg-white rounded-lg border overflow-hidden">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">#</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Customer</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Items</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Total</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Date</th>
                <th className="px-4 py-3" />
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {orders.map(order => (
                <tr key={order.id} className="hover:bg-gray-50 transition-colors">
                  <td className="px-4 py-3 text-sm text-gray-900 font-mono">#{order.id}</td>
                  <td className="px-4 py-3">
                    <p className="text-sm font-medium text-gray-900">{order.customerName}</p>
                    <p className="text-xs text-gray-500">{order.customerPhone}</p>
                  </td>
                  <td className="px-4 py-3 text-sm text-gray-600">{order.itemCount}</td>
                  <td className="px-4 py-3 text-sm font-medium text-gray-900">
                    {order.subtotal.toLocaleString()} {order.currency}
                  </td>
                  <td className="px-4 py-3"><StatusBadge status={order.status} /></td>
                  <td className="px-4 py-3 text-sm text-gray-500">
                    {new Date(order.createdAt).toLocaleDateString()}
                  </td>
                  <td className="px-4 py-3 text-right">
                    <Link
                      href={`/admin/orders/${order.id}`}
                      className="text-sm text-blue-600 hover:text-blue-800 font-medium"
                    >
                      View →
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Pagination */}
      {pageCount > 1 && (
        <div className="flex justify-center gap-2 mt-6">
          {Array.from({ length: pageCount }, (_, i) => i + 1).map(p => (
            <Link
              key={p}
              href={`/admin/orders?${status ? `status=${status}&` : ''}page=${p}`}
              className={`w-8 h-8 flex items-center justify-center rounded text-sm border ${p === page ? 'bg-blue-600 text-white border-blue-600' : 'bg-white text-gray-600 border-gray-300 hover:border-gray-400'}`}
            >
              {p}
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
