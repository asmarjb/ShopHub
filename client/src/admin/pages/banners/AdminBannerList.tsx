import React, { useEffect, useState } from 'react';
import { Plus, Trash2, Edit, Image } from 'lucide-react';
import { adminService } from '../../services/adminService';
import { Banner } from '../../../types';
import toast from 'react-hot-toast';

export const AdminBannerList: React.FC = () => {
  const [banners, setBanners] = useState<Banner[]>([]);
  const [loading, setLoading] = useState(true);

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [title, setTitle] = useState('');
  const [subTitle, setSubTitle] = useState('');
  const [imageUrl, setImageUrl] = useState('');
  const [linkUrl, setLinkUrl] = useState('');
  const [displayOrder, setDisplayOrder] = useState(1);
  const [isActive, setIsActive] = useState(true);

  const fetchBanners = async () => {
    setLoading(true);
    try {
      const data = await adminService.getBanners();
      setBanners(data);
    } catch {
      toast.error('Bannerlər yüklənə bilmədi.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchBanners();
  }, []);

  const handleOpenCreate = () => {
    setEditingId(null);
    setTitle('');
    setSubTitle('');
    setImageUrl('');
    setLinkUrl('');
    setDisplayOrder(1);
    setIsActive(true);
    setIsModalOpen(true);
  };

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const payload = { title, subTitle, imageUrl, linkUrl, displayOrder, isActive };
      if (editingId) {
        await adminService.updateBanner(editingId, payload);
        toast.success('Banner yeniləndi!');
      } else {
        await adminService.createBanner(payload);
        toast.success('Yeni banner yaradıldı!');
      }
      setIsModalOpen(false);
      fetchBanners();
    } catch {
      toast.error('Banner saxlanılarkən xəta baş verdi.');
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm('Banneri silmək istədiyinizdən əminsiniz?')) return;
    try {
      await adminService.deleteBanner(id);
      toast.success('Banner silindi.');
      fetchBanners();
    } catch {
      toast.error('Silinərkən xəta baş verdi.');
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-black text-gray-900 dark:text-gray-100">Bannerlər</h1>
          <p className="text-xs text-gray-500 mt-1">Ana səhifə slaydları və xüsusi kampaniya afişaları</p>
        </div>

        <button
          onClick={handleOpenCreate}
          className="px-4 py-2 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-bold flex items-center gap-1.5 shadow-md shadow-indigo-600/30 transition"
        >
          <Plus className="w-4 h-4" /> Yeni Banner
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {banners.map((b) => (
          <div
            key={b.id}
            className="rounded-3xl bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 overflow-hidden shadow-sm flex flex-col"
          >
            <div className="aspect-[21/9] bg-gray-100 dark:bg-gray-800 relative">
              <img src={b.imageUrl} alt="" className="w-full h-full object-cover" />
              <span
                className={`absolute top-3 right-3 px-2.5 py-1 rounded-full text-[10px] font-bold ${
                  b.isActive ? 'bg-emerald-500 text-white' : 'bg-gray-500 text-white'
                }`}
              >
                {b.isActive ? 'Aktiv' : 'Passiv'}
              </span>
            </div>

            <div className="p-5 flex-1 flex flex-col justify-between space-y-4">
              <div>
                <h3 className="font-bold text-base text-gray-900 dark:text-gray-100">{b.title}</h3>
                {b.subTitle && <p className="text-xs text-gray-400 mt-1">{b.subTitle}</p>}
                {b.linkUrl && <p className="text-[11px] text-indigo-500 font-mono mt-2">Keçid: {b.linkUrl}</p>}
              </div>

              <div className="flex items-center justify-between pt-3 border-t border-gray-100 dark:border-gray-800 text-xs">
                <span className="text-gray-400">Sıra: #{b.displayOrder}</span>
                <button
                  onClick={() => handleDelete(b.id)}
                  className="p-1.5 text-rose-500 hover:bg-rose-50 rounded-lg transition"
                >
                  <Trash2 className="w-4 h-4" />
                </button>
              </div>
            </div>
          </div>
        ))}
      </div>

      {isModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
          <form
            onSubmit={handleSave}
            className="w-full max-w-md bg-white dark:bg-gray-800 rounded-3xl p-6 shadow-2xl space-y-4"
          >
            <h3 className="font-bold text-base text-gray-900 dark:text-gray-100">Yeni Banner</h3>

            <div>
              <label className="block text-xs font-semibold mb-1">Başlıq *</label>
              <input
                type="text"
                required
                value={title}
                onChange={(e) => setTitle(e.target.value)}
                className="w-full px-4 py-2 text-xs rounded-xl border"
              />
            </div>

            <div>
              <label className="block text-xs font-semibold mb-1">Alt Başlıq</label>
              <input
                type="text"
                value={subTitle}
                onChange={(e) => setSubTitle(e.target.value)}
                className="w-full px-4 py-2 text-xs rounded-xl border"
              />
            </div>

            <div>
              <label className="block text-xs font-semibold mb-1">Şəkil URL *</label>
              <input
                type="url"
                required
                value={imageUrl}
                onChange={(e) => setImageUrl(e.target.value)}
                className="w-full px-4 py-2 text-xs rounded-xl border"
              />
            </div>

            <div>
              <label className="block text-xs font-semibold mb-1">Keçid URL-i</label>
              <input
                type="text"
                placeholder="/products?category=..."
                value={linkUrl}
                onChange={(e) => setLinkUrl(e.target.value)}
                className="w-full px-4 py-2 text-xs rounded-xl border"
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
