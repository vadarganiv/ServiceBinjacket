'use client';

import { useState, useRef } from 'react';
import { useRouter } from 'next/navigation';

interface ProductImage {
  id: number;
  path: string;
  altSq: string;
  sortOrder: number;
}

interface Props {
  productId: number;
  images: ProductImage[];
}

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? 'http://localhost:5000';

export default function ProductImagesPanel({ productId, images: initialImages }: Props) {
  const router = useRouter();
  const [images, setImages] = useState(initialImages);
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState('');
  const fileRef = useRef<HTMLInputElement>(null);

  async function handleUpload(e: React.ChangeEvent<HTMLInputElement>) {
    const files = e.target.files;
    if (!files || files.length === 0) return;

    setUploading(true);
    setError('');
    const formData = new FormData();
    for (const f of files) formData.append('files', f);

    const res = await fetch(`${API_URL}/api/v1/admin/products/${productId}/images`, {
      method: 'POST',
      credentials: 'include',
      body: formData,
    });

    setUploading(false);
    if (res.ok) {
      router.refresh();
    } else {
      const data = await res.json().catch(() => ({}));
      setError(data?.error?.message ?? 'Upload failed');
    }

    if (fileRef.current) fileRef.current.value = '';
  }

  async function handleDelete(imageId: number) {
    if (!confirm('Delete this image?')) return;

    const res = await fetch(`${API_URL}/api/v1/admin/products/${productId}/images/${imageId}`, {
      method: 'DELETE',
      credentials: 'include',
    });

    if (res.ok) {
      setImages(prev => prev.filter(i => i.id !== imageId));
    } else {
      setError('Delete failed');
    }
  }

  return (
    <div className="bg-white rounded-lg border p-4">
      <div className="flex items-center justify-between mb-4">
        <h3 className="font-medium text-gray-900">Images ({images.length})</h3>
        <label className="cursor-pointer px-3 py-1.5 bg-blue-600 text-white text-sm rounded-md hover:bg-blue-700 transition-colors">
          {uploading ? 'Uploading...' : 'Upload images'}
          <input
            ref={fileRef}
            type="file"
            accept="image/jpeg,image/png,image/webp"
            multiple
            className="hidden"
            onChange={handleUpload}
            disabled={uploading}
          />
        </label>
      </div>

      {error && <p className="text-sm text-red-600 mb-3">{error}</p>}

      {images.length === 0 ? (
        <p className="text-sm text-gray-400 text-center py-6">No images yet</p>
      ) : (
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
          {images.map(img => {
            const imgUrl = `${API_URL}${img.path}`;
            return (
              <div key={img.id} className="relative group rounded-lg overflow-hidden border bg-gray-50">
                {/* eslint-disable-next-line @next/next/no-img-element */}
                <img
                  src={imgUrl}
                  alt={img.altSq}
                  className="w-full h-28 object-cover"
                />
                <button
                  onClick={() => handleDelete(img.id)}
                  className="absolute top-1 right-1 w-6 h-6 bg-red-600 text-white rounded-full text-xs opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center"
                >
                  ×
                </button>
                <p className="text-xs text-gray-500 px-1 py-1 truncate">#{img.sortOrder}</p>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
