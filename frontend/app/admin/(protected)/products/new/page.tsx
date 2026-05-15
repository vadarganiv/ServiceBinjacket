import { cookies } from 'next/headers';
import { redirect } from 'next/navigation';
import Link from 'next/link';
import { adminFetch } from '@/features/admin/lib/api';
import ProductForm from '@/features/admin/products/ProductForm';

export default async function AdminProductNewPage() {
  const cookieStore = await cookies();
  const token = cookieStore.get('sb_admin_token')?.value;
  if (!token) redirect('/admin/login');

  const catRes = await adminFetch('/admin/products/categories', token);
  if (catRes.status === 401) redirect('/admin/login');
  const categories = await catRes.json();

  return (
    <div>
      <Link href="/admin/products" className="text-sm text-gray-500 hover:text-gray-700 mb-4 inline-block">
        ← Back to products
      </Link>
      <h1 className="text-2xl font-semibold text-gray-900 mb-6">New Product</h1>
      <ProductForm categories={categories} mode="create" />
    </div>
  );
}
