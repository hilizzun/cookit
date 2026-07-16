import ReferenceTable from './ReferenceTable';
import { useAuth } from '../../contexts/AuthContext';
import { API_BASE_URL } from '../../config/settings';

const IngredientsAdmin = () => {
  const { accessToken } = useAuth();

  const handleAutoFill = async (ingredientName) => {
    console.log('Auto-fill requested for:', ingredientName);
    const response = await fetch(`${API_BASE_URL}/api/admin/ingredients/auto-nutrition`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        Authorization: `Bearer ${accessToken}`,
      },
      body: JSON.stringify({ name: ingredientName }),
    });
    console.log('Response status:', response.status);
    if (!response.ok) {
      const errorText = await response.text();
      console.error('Error response:', errorText);
      throw new Error('Не удалось получить КБЖУ');
    }
    const data = await response.json();
    console.log('Received nutrition:', data);
    return data;
  };

  const columns = [
    { key: 'id', label: 'ID' },
    { 
      key: 'name', 
      label: 'Название',
      render: (value) => <span className="truncate max-w-[150px] block">{value}</span>
    },
    { 
      key: 'calories', 
      label: 'Кал',
      render: (value) => value?.toFixed(1) || '0.0'
    },
    { 
      key: 'proteins', 
      label: 'Б',
      render: (value) => value?.toFixed(1) || '0.0'
    },
    { 
      key: 'fats', 
      label: 'Ж',
      render: (value) => value?.toFixed(1) || '0.0'
    },
    { 
      key: 'carbohydrates', 
      label: 'У',
      render: (value) => value?.toFixed(1) || '0.0'
    },
    { 
      key: 'isByPiece', 
      label: 'Шт',
      render: (value) => value ? '✓' : '—'
    },
    { 
      key: 'isUsedInRecipes', 
      label: 'Исп',
      render: (value) => value ? '✓' : '—'
    },
    { 
      key: 'isDeleted', 
      label: 'Статус',
      render: (value) => (
        <span className={`text-xs px-1 py-0.5 rounded ${value ? 'bg-red-100 text-red-800' : 'bg-green-100 text-green-800'}`}>
          {value ? 'Удал' : 'Акт'}
        </span>
      )
    },
  ];

  const formFields = [
    { name: 'name', label: 'Название ингредиента', type: 'text', required: true },
    { name: 'calories', label: 'Калории (на 100г)', type: 'number', step: '0.1', required: true },
    { name: 'proteins', label: 'Белки (г)', type: 'number', step: '0.1', required: true },
    { name: 'fats', label: 'Жиры (г)', type: 'number', step: '0.1', required: true },
    { name: 'carbohydrates', label: 'Углеводы (г)', type: 'number', step: '0.1', required: true },
    { name: 'isByPiece', label: 'Измеряется поштучно', type: 'checkbox' },
  ];

  const transformData = (data) => ({
    name: data.name || '',
    calories: data.calories || 0,
    proteins: data.proteins || 0,
    fats: data.fats || 0,
    carbohydrates: data.carbohydrates || 0,
    isByPiece: data.isByPiece || false,
  });

  return (
    <ReferenceTable
      title="Ингредиенты"
      endpoint="ingredients"
      columns={columns}
      formFields={formFields}
      transformData={transformData}
      accessToken={accessToken}
      onAutoFill={handleAutoFill}
      autoFillFieldName="name"
    />
  );
};

export default IngredientsAdmin;