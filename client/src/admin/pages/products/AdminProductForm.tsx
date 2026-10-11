import React, { useEffect, useState } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import { ArrowLeft, Plus, Trash2, Upload, Check } from 'lucide-react';
import { adminService, UpsertProductDto } from '../../services/adminService';
import { Category, Brand } from '../../../types';
import toast from 'react-hot-toast';

export const AdminProductForm: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const isEditing = !!id;

  const [categories, setCategories] = useState<Category[]>([]);
  const [brands, setBrands] = useState<Brand[]>([]);
  const [loading, setLoading] = useState(false);

  // Form State
  const [formData, setFormData] = useState<UpsertProductDto>({
    name: '',
    sku: '',
    basePrice: 0,
    discountPrice: undefined,
    stockQuantity: 10,
    categoryId: '',
    brandId: undefined,
    shortDescription: '',
    description: '',
    isFeatured: false,
    imageUrls: [],
    primaryImageUrl: '',
    tags: [],
    variants: [],
  });

  const [imageUrlInput, setImageUrlInput] = useState('');
  const [tagInput, setTagInput] = useState('');

  // Variant helper state
  const [variantSku, setVariantSku] = useState('');
  const [variantPrice, setVariantPrice] = useState(0);
  const [variantStock, setVariantStock] = useState(5);
  const [variantAttrKey, setVariantAttrKey] = useState('Ölçü');
  const [variantAttrVal, setVariantAttrVal] = useState('M');

  useEffect(() => {
    const initData = async () => {
      try {
        const [cats, brs] = await Promise.all([
          adminService.getCategories(),
          adminService.getBrands(),
        ]);
        setCategories(cats);
        setBrands(brs);

        if (isEditing) {
          const prod = await adminService.getProductById(id);
          setFormData({
            name: prod.name,
            sku: prod.sku,
            basePrice: prod.basePrice,
            discountPrice: prod.discountPrice,
            stockQuantity: prod.stockQuantity,
            categoryId: prod.categoryId,
            brandId: prod.brandId,
            shortDescription: prod.shortDescription || '',
            description: prod.description || '',
            isFeatured: prod.isFeatured,
            imageUrls: prod.images?.map((i) => i.imageUrl) || [],
            primaryImageUrl: prod.primaryImageUrl || '',
            tags: prod.tags || [],
            variants: prod.variants?.map((v) => ({
              sku: v.sku,
              price: v.price,
              stockQuantity: v.stockQuantity,
              attributes: v.attributes,
            })) || [],
          });
        } else if (cats.length > 0) {
          setFormData((prev) => ({ ...prev, categoryId: cats[0].id }));
        }
      } catch {
        toast.error('Məlumatlar yüklənərkən xəta baş verdi.');
      }
    };

    initData();
  }, [id]);

  const handleAddImage = () => {
    if (!imageUrlInput.trim()) return;
    setFormData((prev) => {
      const urls = [...(prev.imageUrls || []), imageUrlInput.trim()];
      return {
        ...prev,
        imageUrls: urls,
        primaryImageUrl: prev.primaryImageUrl || imageUrlInput.trim(),
      };
    });
    setImageUrlInput('');
  };

  const handleRemoveImage = (index: number) => {
    setFormData((prev) => {
      const urls = (prev.imageUrls || []).filter((_, i) => i !== index);
      return {
        ...prev,
        imageUrls: urls,
        primaryImageUrl: urls[0] || '',
      };
    });
  };

  const handleAddTag = () => {
    if (!tagInput.trim()) return;
    setFormData((prev) => ({
      ...prev,
      tags: [...(prev.tags || []), tagInput.trim()],
    }));
    setTagInput('');
  };

  const handleRemoveTag = (index: number) => {
    setFormData((prev) => ({
      ...prev,
      tags: (prev.tags || []).filter((_, i) => i !== index),
    }));
  };

  const handleAddVariant = () => {
    if (!variantSku.trim()) {
      toast.error('Variant üçün SKU qeyd edin.');
      return;
    }
    setFormData((prev) => ({
      ...prev,
      variants: [
        ...(prev.variants || []),
        {
          sku: variantSku.trim(),
          price: variantPrice || prev.basePrice,
          stockQuantity: variantStock,
          attributes: { [variantAttrKey]: variantAttrVal },
        },
      ],
    }));
    setVariantSku('');
  };

  const handleRemoveVariant = (index: number) => {
    setFormData((prev) => ({
      ...prev,
      variants: (prev.variants || []).filter((_, i) => i !== index),
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      if (isEditing) {
        await adminService.updateProduct(id, formData);
        toast.success('Məhsul uğurla yeniləndi!');
      } else {
        await adminService.createProduct(formData);
        toast.success('Yeni məhsul uğurla yaradıldı!');
      }
      navigate('/admin/products');
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Yadda saxlanılarkən xəta baş verdi.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <form onSubmit={handleSubmit} className="max-w-4xl mx-auto space-y-8 pb-12">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <Link
            to="/admin/products"
            className="inline-flex items-center gap-1.5 text-xs text-indigo-600 font-semibold mb-2 hover:underline"
          >
            <ArrowLeft className="w-4 h-4" /> Məhsullara Qayıt
          </Link>
          <h1 className="text-2xl font-black text-gray-900 dark:text-gray-100">
            {isEditing ? 'Məhsulu Redaktə Et' : 'Yeni Məhsul Əlavə Et'}
          </h1>
        </div>

        <button
          type="submit"
          disabled={loading}
          className="px-6 py-2.5 rounded-xl bg-indigo-600 hover:bg-indigo-700 text-white font-bold text-xs shadow-md shadow-indigo-600/30 transition disabled:opacity-50"
        >
          {loading ? 'Saxlanılır...' : 'Yadda Saxla'}
        </button>
      </div>

      {/* Basic Info */}
      <div className="p-6 rounded-3xl bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 shadow-sm space-y-4">
        <h3 className="text-sm font-bold uppercase tracking-wider text-gray-500">
          Əsas Məlumatlar
        </h3>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label className="block text-xs font-semibold mb-1">Məhsulun Adı *</label>
            <input
              type="text"
              required
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
              className="w-full px-4 py-2 text-xs rounded-xl border border-gray-300 dark:border-gray-700 bg-gray-50 dark:bg-gray-800"
            />
          </div>

          <div>
            <label className="block text-xs font-semibold mb-1">Artikul / SKU *</label>
            <input
              type="text"
              required
              value={formData.sku}
              onChange={(e) => setFormData({ ...formData, sku: e.target.value })}
              className="w-full px-4 py-2 text-xs rounded-xl border border-gray-300 dark:border-gray-700 bg-gray-50 dark:bg-gray-800 font-mono"
            />
          </div>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <label className="block text-xs font-semibold mb-1">Kateqoriya *</label>
            <select
              value={formData.categoryId}
              onChange={(e) => setFormData({ ...formData, categoryId: e.target.value })}
              className="w-full px-4 py-2 text-xs rounded-xl border border-gray-300 dark:border-gray-700 bg-gray-50 dark:bg-gray-800"
            >
              {categories.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-xs font-semibold mb-1">Brend</label>
            <select
              value={formData.brandId || ''}
              onChange={(e) =>
                setFormData({ ...formData, brandId: e.target.value || undefined })
              }
              className="w-full px-4 py-2 text-xs rounded-xl border border-gray-300 dark:border-gray-700 bg-gray-50 dark:bg-gray-800"
            >
              <option value="">Brend yoxdur</option>
              {brands.map((b) => (
                <option key={b.id} value={b.id}>
                  {b.name}
                </option>
              ))}
            </select>
          </div>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <div>
            <label className="block text-xs font-semibold mb-1">Əsas Qiymət ($) *</label>
            <input
              type="number"
              step="0.01"
              required
              value={formData.basePrice}
              onChange={(e) =>
                setFormData({ ...formData, basePrice: parseFloat(e.target.value) || 0 })
              }
              className="w-full px-4 py-2 text-xs rounded-xl border border-gray-300 dark:border-gray-700 bg-gray-50 dark:bg-gray-800"
            />
          </div>

          <div>
            <label className="block text-xs font-semibold mb-1">Endirimli Qiymət ($)</label>
            <input
              type="number"
              step="0.01"
              value={formData.discountPrice || ''}
              onChange={(e) =>
                setFormData({
                  ...formData,
                  discountPrice: e.target.value ? parseFloat(e.target.value) : undefined,
                })
              }
              className="w-full px-4 py-2 text-xs rounded-xl border border-gray-300 dark:border-gray-700 bg-gray-50 dark:bg-gray-800"
            />
          </div>

          <div>
            <label className="block text-xs font-semibold mb-1">Ümumi Stok Sayı *</label>
            <input
              type="number"
              required
              value={formData.stockQuantity}
              onChange={(e) =>
                setFormData({ ...formData, stockQuantity: parseInt(e.target.value, 10) || 0 })
              }
              className="w-full px-4 py-2 text-xs rounded-xl border border-gray-300 dark:border-gray-700 bg-gray-50 dark:bg-gray-800"
            />
          </div>
        </div>

        <div>
          <label className="block text-xs font-semibold mb-1">Qısa Təsvir</label>
          <input
            type="text"
            value={formData.shortDescription}
            onChange={(e) => setFormData({ ...formData, shortDescription: e.target.value })}
            className="w-full px-4 py-2 text-xs rounded-xl border border-gray-300 dark:border-gray-700 bg-gray-50 dark:bg-gray-800"
          />
        </div>

        <div>
          <label className="block text-xs font-semibold mb-1">Geniş Təsvir</label>
          <textarea
            rows={4}
            value={formData.description}
            onChange={(e) => setFormData({ ...formData, description: e.target.value })}
            className="w-full px-4 py-2 text-xs rounded-xl border border-gray-300 dark:border-gray-700 bg-gray-50 dark:bg-gray-800"
          />
        </div>

        <div className="flex items-center gap-2 pt-2">
          <input
            type="checkbox"
            id="featured"
            checked={formData.isFeatured}
            onChange={(e) => setFormData({ ...formData, isFeatured: e.target.checked })}
            className="rounded border-gray-300 text-indigo-600 focus:ring-indigo-500"
          />
          <label htmlFor="featured" className="text-xs font-semibold text-gray-700 dark:text-gray-300">
            Seçilmiş məhsul olaraq ana səhifədə göstər
          </label>
        </div>
      </div>

      {/* Images Upload / URLs */}
      <div className="p-6 rounded-3xl bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 shadow-sm space-y-4">
        <h3 className="text-sm font-bold uppercase tracking-wider text-gray-500">
          Məhsul Şəkilləri
        </h3>

        <div className="flex gap-2">
          <input
            type="url"
            placeholder="Şəkil URL-i daxil edin..."
            value={imageUrlInput}
            onChange={(e) => setImageUrlInput(e.target.value)}
            className="flex-1 px-4 py-2 text-xs rounded-xl border border-gray-300 dark:border-gray-700 bg-gray-50 dark:bg-gray-800"
          />
          <button
            type="button"
            onClick={handleAddImage}
            className="px-4 py-2 rounded-xl bg-gray-800 text-white text-xs font-bold hover:bg-gray-900"
          >
            Əlavə et
          </button>
        </div>

        <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 pt-2">
          {formData.imageUrls?.map((url, idx) => (
            <div
              key={idx}
              className={`relative rounded-2xl overflow-hidden border-2 aspect-square group ${
                formData.primaryImageUrl === url ? 'border-indigo-600' : 'border-gray-200'
              }`}
            >
              <img src={url} alt="" className="w-full h-full object-cover" />
              <div className="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 transition flex items-center justify-center gap-2">
                <button
                  type="button"
                  title="Əsas şəkil et"
                  onClick={() => setFormData({ ...formData, primaryImageUrl: url })}
                  className="p-1.5 rounded-lg bg-white/90 text-indigo-600 hover:scale-110"
                >
                  <Check className="w-3.5 h-3.5" />
                </button>
                <button
                  type="button"
                  title="Sil"
                  onClick={() => handleRemoveImage(idx)}
                  className="p-1.5 rounded-lg bg-white/90 text-rose-600 hover:scale-110"
                >
                  <Trash2 className="w-3.5 h-3.5" />
                </button>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Variants Section */}
      <div className="p-6 rounded-3xl bg-white dark:bg-gray-900 border border-gray-200 dark:border-gray-800 shadow-sm space-y-4">
        <h3 className="text-sm font-bold uppercase tracking-wider text-gray-500">
          Məhsul Variantları (Ölçü, Rəng və s.)
        </h3>

        <div className="grid grid-cols-2 sm:grid-cols-5 gap-3 p-4 bg-gray-50 dark:bg-gray-800/50 rounded-2xl">
          <input
            type="text"
            placeholder="SKU (məs: SHIRT-M)"
            value={variantSku}
            onChange={(e) => setVariantSku(e.target.value)}
            className="px-3 py-1.5 text-xs rounded-xl border bg-white dark:bg-gray-900 font-mono"
          />
          <input
            type="number"
            placeholder="Qiymət"
            value={variantPrice || ''}
            onChange={(e) => setVariantPrice(parseFloat(e.target.value) || 0)}
            className="px-3 py-1.5 text-xs rounded-xl border bg-white dark:bg-gray-900"
          />
          <input
            type="number"
            placeholder="Stok"
            value={variantStock}
            onChange={(e) => setVariantStock(parseInt(e.target.value, 10) || 0)}
            className="px-3 py-1.5 text-xs rounded-xl border bg-white dark:bg-gray-900"
          />
          <input
            type="text"
            placeholder="Dəyər (M / Qara)"
            value={variantAttrVal}
            onChange={(e) => setVariantAttrVal(e.target.value)}
            className="px-3 py-1.5 text-xs rounded-xl border bg-white dark:bg-gray-900"
          />
          <button
            type="button"
            onClick={handleAddVariant}
            className="px-3 py-1.5 bg-indigo-600 text-white text-xs font-bold rounded-xl"
          >
            Variant əlavə et
          </button>
        </div>

        {formData.variants && formData.variants.length > 0 && (
          <div className="space-y-2">
            {formData.variants.map((v, i) => (
              <div
                key={i}
                className="flex items-center justify-between p-3 rounded-xl border border-gray-200 dark:border-gray-700 text-xs"
              >
                <div>
                  <span className="font-mono font-bold">{v.sku}</span> · ${v.price} · {v.stockQuantity} ədəd · {JSON.stringify(v.attributes)}
                </div>
                <button
                  type="button"
                  onClick={() => handleRemoveVariant(i)}
                  className="text-rose-500 hover:text-rose-700"
                >
                  <Trash2 className="w-4 h-4" />
                </button>
              </div>
            ))}
          </div>
        )}
      </div>
    </form>
  );
};
