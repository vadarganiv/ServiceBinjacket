'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';

interface Props {
  productId: number;
  isPublished: boolean;
}

export default function PublishToggle({ productId, isPublished }: Props) {
  const router = useRouter();
  const [loading, setLoading] = useState(false);
  const [published, setPublished] = useState(isPublished);

  async function toggle() {
    setLoading(true);
    const action = published ? 'unpublish' : 'publish';
    const res = await fetch(`/api/v1/admin/products/${productId}/${action}`, {
      method: 'POST',
      credentials: 'include',
    });
    setLoading(false);
    if (res.ok) {
      setPublished(p => !p);
      router.refresh();
    }
  }

  return (
    <button
      onClick={toggle}
      disabled={loading}
      className={`px-4 py-2 text-sm font-medium rounded-lg border transition-colors disabled:opacity-50 ${
        published
          ? 'bg-white border-red-300 text-red-600 hover:bg-red-50'
          : 'bg-white border-green-300 text-green-700 hover:bg-green-50'
      }`}
    >
      {loading ? '...' : published ? 'Unpublish' : 'Publish'}
    </button>
  );
}
