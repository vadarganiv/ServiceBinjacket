import { cookies } from 'next/headers';
import { redirect } from 'next/navigation';
import Link from 'next/link';
import { adminFetch } from '@/features/admin/lib/api';

interface ProductItem {
  id: number;
  nameSq: string;
  nameEn: string | null;
  slugSq: string;
  price: number;
  currency: string;
  condition: string;
  stockQty: number | null;
  isPublished: boolean;
  categoryName: string;
  imageCount: number;
  createdAt: string;
}

export default async function AdminProductsPage({
  searchParams,
}: {
  searchParams: Promise<{ q?: string; isPublished?: string; page?: string }>;
}) {
  const params = await searchParams;
  const cookieStore = await cookies();
  const token = cookieStore.get('sb_admin_token')?.value;
  if (!token) redirect('/admin/login');

  const page = parseInt(params.page ?? '1', 10);
  const qs = new URLSearchParams({ page: String(page), pageSize: '20' });
  if (params.q) qs.set('q', params.q);
  if (params.isPublished !== undefined) qs.set('isPublished', params.isPublished);

  const res = await adminFetch(`/admin/products?${qs}`, token);
  if (res.status === 401) redirect('/admin/login');

  const data = await res.json();
  const products: ProductItem[] = data.items ?? [];
  const total: number = data.totalCount ?? 0;
  const pageCount = Math.ceil(total / 20);

  const publishFilter = params.isPublished;

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-semibold text-gray-900">Products</h1>
        <Link
          href="/admin/products/new"
          className="px-4 py-2 bg-blue-600 text-white text-sm font-medium rounded-lg hover:bg-blue-700 transition-colors"
        >
          + Add product
        </Link>
      </div>

      {/* Filters */}
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
          <Link
            href="/admin/products"
            className={`px-3 py-1.5 rounded-md text-sm border ${!publishFilter ? 'bg-blue-600 text-white border-blue-600' : 'bg-white text-gray-600 border-gray-300'}`}
          >
            All
          </Link>
          <Link
            href="/admin/products?isPublished=true"
            className={`px-3 py-1.5 rounded-md text-sm border ${publishFilter === 'true' ? 'bg-blue-600 text-white border-blue-600' : 'bg-white text-gray-600 border-gray-300'}`}
          >
            Published
          </Link>
          <Link
            href="/admin/products?isPublished=false"
            className={`px-3 py-1.5 rounded-md text-sm border ${publishFilter === 'false' ? 'bg-blue-600 text-white border-blue-600' : 'bg-white text-gray-600 border-gray-300'}`}
          >
            Draft
          </Link>
        </div>
        <span className="text-sm text-gray-500 self-center">{total} total</span>
      </div>

      {products.length === 0 ? (
        <div className="bg-white rounded-lg border p-8 text-center text-gray-400">No products found</div>
      ) : (
        <div className="bg-white rounded-lg border overflow-hidden">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">#</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Name</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Category</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Price</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Condition</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Stock</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Imgs</th>
                <th className="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase">Status</th>
                <th className="px-4 py-3" />
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {products.map(p => (
                <tr key={p.id} className="hover:bg-gray-50 transition-colors">
                  <td className="px-4 py-3 text-sm text-gray-500 font-mono">{p.id}</td>
                  <td className="px-4 py-3">
                    <p className="text-sm font-medium text-gray-900">{p.nameSq}</p>
                    {p.nameEn && <p className="text-xs text-gray-500">{p.nameEn}</p>}
                  </td>
                  <td className="px-4 py-3 text-sm text-gray-600">{p.categoryName}</td>
                  <td className="px-4 py-3 text-sm text-gray-900 font-medium">
                    {p.price.toLocaleString()} {p.currency}
                  </td>
                  <td className="px-4 py-3 text-sm text-gray-600">{p.condition}</td>
                  <td className="px-4 py-3 text-sm text-gray-600">
                    {p.stockQty === null ? '∞' : p.stockQty}
                  </td>
                  <td className="px-4 py-3 text-sm text-gray-600">{p.imageCount}</td>
                  <td className="px-4 py-3">
                    <span className={`inline-flex px-2 py-0.5 rounded text-xs font-medium ${p.isPublished ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-600'}`}>
                      {p.isPublished ? 'Published' : 'Draft'}
                    </span>
                  </td>
                  <td className="px-4 py-3 text-right">
                    <Link
                      href={`/admin/products/${p.id}`}
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

      {pageCount > 1 && (
        <div className="flex justify-center gap-2 mt-6">
          {Array.from({ length: pageCount }, (_, i) => i + 1).map(p => (
            <Link
              key={p}
              href={`/admin/products?${params.q ? `q=${params.q}&` : ''}${publishFilter ? `isPublished=${publishFilter}&` : ''}page=${p}`}
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
