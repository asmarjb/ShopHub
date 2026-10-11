import React, { useEffect, useState } from 'react';
import { Plus, Trash2, Edit } from 'lucide-react';
import { adminService } from '../../services/adminService';
import { Brand } from '../../../types';
import toast from 'react-hot-toast';

export const AdminBrandList: React.FC = () => {
  const [brands, setBrands] = useState<Brand[]>([]);
  const [loading, setLoading] = useState(true);

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [name, setName] = useState('');
  const [slug, setSlug] = useState('');
  const [logoUrl, setLogoUrl] = useState('');

  const fetchBrands = async () => {
    setLoading(true);
    try {
      const data = await adminService.getBrands();
      setBrands(data);
    } catch {
      toast.error('Brendlər yüklənə bilmədi.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchBrands();
  }, []);

  const handleOpenCreate = () => {
    setEditingId(null);
    setName('');
    setSlug('');
    setLogoUrl('');
    setIsModalOpen(true);
  };

  const handleOpenEdit = (b: Brand) => {
    setEditingId(b.id);
    setName(b.name);
    setSlug(b.slug);
    setLogoUrl(b.logoUrl || '');
    setIsModalOpen(true);
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (editingId) {
        await adminService.updateBrand(editingId, { name, slug, logoUrl });
        toast.success('Brend yeniləndi!');
      } else {
        await adminService.createBrand({ name, slug, logoUrl });
        toast.success('Yeni brend yaradıldı!');
      }
      setIsModalOpen(false);
      fetchBrands();
    } catch {
      toast.error('Əməliyyat zamanı xəta baş verdi.');
    }
  };

  const handleDelete = async (id: string, name: string) => {
    if (!window.confirm(`"${name}" brendini silmək istədiyinizdən əminsiniz?`)) return;
    try {
      await adminService.deleteBrand(id);
      toast.success('Brend silindi.');
      fetchBrands();
    } catch {
      toast.error('Brend silinərkən xəta baş verdi.');
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-black text-gray-900 dark:text-gray-100">Brendlər</h1>
          <p className="text-xs text-gray-500 mt-1">Məhsul markaları və tərəfdaş brendlər</p>
        </div>

        <button
          onClick={handleOpenCreate}
          className="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-bold flex items-center gap-1.5 shadow-md shadow-indigo-600/30 transition"
        >
          <Plus className="w-4 h-4" /> Yeni Brend
        </button>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
        {brands.map((b) => (
          <div
            key={b.id}
            className="p-5 rounded-2xl bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 shadow-sm flex items-center justify-between"
          >
            <div>
              <h3 className="font-bold text-sm text-gray-900 dark:text-gray-100">{b.name}</h3>
              <p className="text-[10px] text-gray-400 font-mono">{b.slug}</p>
            </div>

            <div className="flex gap-1">
              <button
                onClick={() => handleOpenEdit(b)}
                className="p-1.5 text-indigo-600 hover:bg-indigo-50 rounded-lg transition"
              >
                <Edit className="w-4 h-4" />
              </button>
              <button
                onClick={() => handleDelete(b.id, b.name)}
                className="p-1.5 text-rose-500 hover:bg-rose-50 rounded-lg transition"
              >
                <Trash2 className="w-4 h-4" />
              </button>
            </div>
          </div>
        ))}
      </div>

      {isModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
          <form
            onSubmit={handleSave}
            className="w-full max-w-sm bg-white dark:bg-gray-800 rounded-3xl p-6 shadow-2xl space-y-4"
          >
            <h3 className="font-bold text-base text-gray-900 dark:text-gray-100">
              {editingId ? 'Brendi Redaktə Et' : 'Yeni Brend'}
            </h3>

            <div>
              <label className="block text-xs font-semibold mb-1">Brend Adı *</label>
              <input
                type="text"
                required
                value={name}
                onChange={(e) => setName(e.target.value)}
                className="w-full px-4 py-2 text-xs rounded-xl border bg-gray-50 dark:bg-gray-900"
              />
            </div>

            <div>
              <label className="block text-xs font-semibold mb-1">Logo URL</label>
              <input
                type="url"
                value={logoUrl}
                onChange={(e) => setLogoUrl(e.target.value)}
                className="w-full px-4 py-2 text-xs rounded-xl border bg-gray-50 dark:bg-gray-900"
              />
            </div>

            <div className="flex gap-2 pt-2">
              <button
                type="button"
                onClick={() => setIsModalOpen(false)}
                className="flex-1 py-2 text-xs rounded-xl border font-semibold"
              >
                Ləğv et
              </button>
              <button
                type="submit"
                className="flex-1 py-2 text-xs rounded-xl bg-indigo-600 text-white font-bold"
              >
                Yadda saxla
              </button>
            </div>
          </form>
        </div>
      )}
    </div>
  );
};
