import React, { useEffect, useState } from 'react';
import { Plus, Trash2, Edit, AlertCircle, ArrowRight } from 'lucide-react';
import { adminService } from '../../services/adminService';
import { Category } from '../../../types';
import toast from 'react-hot-toast';

export const AdminCategoryList: React.FC = () => {
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);

  // New/Edit Modal state
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [name, setName] = useState('');
  const [slug, setSlug] = useState('');
  const [description, setDescription] = useState('');
  const [parentId, setParentId] = useState<string | undefined>(undefined);

  // Reassign / Delete modal state
  const [deleteTarget, setDeleteTarget] = useState<Category | null>(null);
  const [reassignCategoryId, setReassignCategoryId] = useState<string>('');
  const [deleting, setDeleting] = useState(false);

  const fetchCategories = async () => {
    setLoading(true);
    try {
      const data = await adminService.getCategories();
      setCategories(data);
    } catch {
      toast.error('Kateqoriyalar yüklənə bilmədi.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchCategories();
  }, []);

  const handleOpenCreate = () => {
    setEditingId(null);
    setName('');
    setSlug('');
    setDescription('');
    setParentId(undefined);
    setIsModalOpen(true);
  };

  const handleOpenEdit = (cat: Category) => {
    setEditingId(cat.id);
    setName(cat.name);
    setSlug(cat.slug || '');
    setDescription(cat.description || '');
    setParentId(cat.parentId || undefined);
    setIsModalOpen(true);
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      if (editingId) {
        await adminService.updateCategory(editingId, { name, slug, description, parentId });
        toast.success('Kateqoriya yeniləndi!');
      } else {
        await adminService.createCategory({ name, slug, description, parentId });
        toast.success('Yeni kateqoriya yaradıldı!');
      }
      setIsModalOpen(false);
      fetchCategories();
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Əməliyyat zamanı xəta baş verdi.');
    }
  };

  const confirmDelete = async () => {
    if (!deleteTarget) return;
    setDeleting(true);
    try {
      await adminService.deleteCategory(
        deleteTarget.id,
        reassignCategoryId || undefined
      );
      toast.success('Kateqoriya silindi.');
      setDeleteTarget(null);
      setReassignCategoryId('');
      fetchCategories();
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Kateqoriyanı silmək mümkün olmadı.');
    } finally {
      setDeleting(false);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-black text-gray-900 dark:text-gray-100">Kateqoriyalar</h1>
          <p className="text-xs text-gray-500 mt-1">
            Bölmələrin iyerarxiyası və məhsul sayları
          </p>
        </div>

        <button
          onClick={handleOpenCreate}
          className="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-bold flex items-center gap-1.5 shadow-md shadow-indigo-600/30 transition"
        >
          <Plus className="w-4 h-4" /> Yeni Kateqoriya
        </button>
      </div>

      {/* Category List Cards/Table */}
      <div className="bg-white dark:bg-gray-900 rounded-3xl border border-gray-200 dark:border-gray-800 overflow-hidden shadow-sm">
        <table className="w-full text-left text-xs">
          <thead className="bg-gray-50 dark:bg-gray-800/60 text-gray-500 uppercase text-[10px] tracking-wider border-b border-gray-200 dark:border-gray-800">
            <tr>
              <th className="px-6 py-4 font-bold">Ad</th>
              <th className="px-6 py-4 font-bold">Slug</th>
              <th className="px-6 py-4 font-bold">Məhsul Sayı</th>
              <th className="px-6 py-4 font-bold text-right">Əməliyyatlar</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-100 dark:divide-gray-800">
            {categories.map((c) => (
              <tr key={c.id} className="hover:bg-gray-50/50 dark:hover:bg-gray-800/30">
                <td className="px-6 py-4 font-bold text-gray-900 dark:text-gray-100">
                  {c.name}
                  {c.children && c.children.length > 0 && (
                    <span className="text-[10px] text-gray-400 block font-normal">
                      {c.children.length} alt kateqoriya
                    </span>
                  )}
                </td>
                <td className="px-6 py-4 font-mono text-gray-500">{c.slug}</td>
                <td className="px-6 py-4 font-bold text-gray-700 dark:text-gray-300">
                  {c.productCount ?? 0}
                </td>
                <td className="px-6 py-4 text-right space-x-2">
                  <button
                    onClick={() => handleOpenEdit(c)}
                    className="p-1.5 text-indigo-600 hover:bg-indigo-50 rounded-lg inline-block transition"
                  >
                    <Edit className="w-4 h-4" />
                  </button>
                  <button
                    onClick={() => setDeleteTarget(c)}
                    className="p-1.5 text-rose-500 hover:bg-rose-50 rounded-lg inline-block transition"
                  >
                    <Trash2 className="w-4 h-4" />
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Create / Edit Modal */}
      {isModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
          <form
            onSubmit={handleSave}
            className="w-full max-w-md bg-white dark:bg-gray-800 rounded-3xl p-6 shadow-2xl space-y-4"
          >
            <h3 className="font-bold text-base text-gray-900 dark:text-gray-100">
              {editingId ? 'Kateqoriyanı Redaktə Et' : 'Yeni Kateqoriya'}
            </h3>

            <div>
              <label className="block text-xs font-semibold mb-1">Kateqoriya Adı *</label>
              <input
                type="text"
                required
                value={name}
                onChange={(e) => setName(e.target.value)}
                className="w-full px-4 py-2 text-xs rounded-xl border bg-gray-50 dark:bg-gray-900"
              />
            </div>

            <div>
              <label className="block text-xs font-semibold mb-1">Slug (istəyə görə)</label>
              <input
                type="text"
                value={slug}
                onChange={(e) => setSlug(e.target.value)}
                className="w-full px-4 py-2 text-xs rounded-xl border bg-gray-50 dark:bg-gray-900 font-mono"
              />
            </div>

            <div>
              <label className="block text-xs font-semibold mb-1">Təsvir</label>
              <textarea
                rows={2}
                value={description}
                onChange={(e) => setDescription(e.target.value)}
                className="w-full px-4 py-2 text-xs rounded-xl border bg-gray-50 dark:bg-gray-900"
              />
            </div>

            <div>
              <label className="block text-xs font-semibold mb-1">Üst Kateqoriya</label>
              <select
                value={parentId || ''}
                onChange={(e) => setParentId(e.target.value || undefined)}
                className="w-full px-4 py-2 text-xs rounded-xl border bg-gray-50 dark:bg-gray-900"
              >
                <option value="">Əsas kateqoriya (Üst kateqoriya yoxdur)</option>
                {categories
                  .filter((c) => c.id !== editingId)
                  .map((c) => (
                    <option key={c.id} value={c.id}>
                      {c.name}
                    </option>
                  ))}
              </select>
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

      {/* Delete / Reassign Modal */}
      {deleteTarget && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
          <div className="w-full max-w-md bg-white dark:bg-gray-800 rounded-3xl p-6 shadow-2xl space-y-4">
            <div className="flex items-center gap-3 text-rose-500">
              <AlertCircle className="w-6 h-6 flex-shrink-0" />
              <h3 className="font-bold text-base text-gray-900 dark:text-gray-100">
                Kateqoriyanı Sil: "{deleteTarget.name}"
              </h3>
            </div>

            <p className="text-xs text-gray-500 leading-relaxed">
              Bu kateqoriyaya bağlı olan məhsulları başqa bir kateqoriyaya yönləndirmək
              istəyirsinizsə, aşağıdan yeni kateqoriyanı seçin. Əks halda silmə əməliyyatı sistem
              tərəfindən məhdudlaşdırıla bilər.
            </p>

            <div>
              <label className="block text-xs font-semibold mb-1">
                Məhsulları Köçür (Reassign)
              </label>
              <select
                value={reassignCategoryId}
                onChange={(e) => setReassignCategoryId(e.target.value)}
                className="w-full px-4 py-2 text-xs rounded-xl border bg-gray-50 dark:bg-gray-900"
              >
                <option value="">Seçilməyib (Birbaşa silməyə cəhd et)</option>
                {categories
                  .filter((c) => c.id !== deleteTarget.id)
                  .map((c) => (
                    <option key={c.id} value={c.id}>
                      {c.name}
                    </option>
                  ))}
              </select>
            </div>

            <div className="flex gap-2 pt-2">
              <button
                type="button"
                onClick={() => setDeleteTarget(null)}
                className="flex-1 py-2 text-xs rounded-xl border font-semibold"
              >
                Ləğv et
              </button>
              <button
                type="button"
                disabled={deleting}
                onClick={confirmDelete}
                className="flex-1 py-2 text-xs rounded-xl bg-rose-600 text-white font-bold disabled:opacity-50"
              >
                {deleting ? 'Silinir...' : 'Təsdiqlə və Sil'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
