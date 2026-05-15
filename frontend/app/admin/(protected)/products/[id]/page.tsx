import { cookies } from 'next/headers';
import { redirect, notFound } from 'next/navigation';
import Link from 'next/link';
import { adminFetch } from '@/features/admin/lib/api';
import ProductForm from '@/features/admin/products/ProductForm';
import ProductImagesPanel from '@/features/admin/products/ProductImagesPanel';
import PublishToggle from '@/features/admin/products/PublishToggle';

export default async function AdminProductEditPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  const cookieStore = await cookies();
  const token = cookieStore.get('sb_admin_token')?.value;
  if (!token) redirect('/admin/login');

  const [productRes, catRes] = await Promise.all([
    adminFetch(`/admin/products/${id}`, token),
    adminFetch('/admin/products/categories', token),
  ]);

  if (productRes.status === 401) redirect('/admin/login');
  if (productRes.status === 404) notFound();

  const product = await productRes.json();
  const categories = await catRes.json();

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <Link href="/admin/products" className="text-sm text-gray-500 hover:text-gray-700">
          ← Back to products
        </Link>
        <PublishToggle productId={product.id} isPublished={product.isPublished} />
      </div>

      <div className="flex items-center gap-3 mb-6">
        <h1 className="text-2xl font-semibold text-gray-900">Edit: {product.nameSq}</h1>
        <span className={`text-xs px-2 py-0.5 rounded font-medium ${product.isPublished ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-600'}`}>
          {product.isPublished ? 'Published' : 'Draft'}
        </span>
      </div>

      <div className="space-y-6">
        <ProductForm
          categories={categories}
          initialData={{ ...product }}
          mode="edit"
        />
        <ProductImagesPanel productId={product.id} images={product.images ?? []} />
      </div>
    </div>
  );
}
