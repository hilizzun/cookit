import ReferenceTable from './ReferenceTable';
import { useAuth } from '../../contexts/AuthContext';

const UnitsAdmin = () => {
  const { accessToken } = useAuth();

  const columns = [
    { key: 'id', label: 'ID' },
    { key: 'name', label: 'Название' },
    { key: 'conversionToGrams', label: 'Конвертация в граммы' },
    { 
      key: 'isUsedInRecipes', 
      label: 'Используется в рецептах',
      render: (value) => value ? 'Да' : 'Нет'
    },
    { 
      key: 'isDeleted', 
      label: 'Статус',
      render: (value) => value ? 'Удален' : 'Активен'
    },
  ];

  const formFields = [
    { name: 'name', label: 'Название единицы измерения', type: 'text', required: true },
    { name: 'conversionToGrams', label: 'Конвертация в граммы (оставьте пустым, если не применимо)', type: 'number', step: '0.1' },
  ];

  const transformData = (data) => ({
    name: data.name || '',
    conversionToGrams: data.conversionToGrams || null,
  });

  return (
    <ReferenceTable
      title="Единицы измерения"
      endpoint="units"
      columns={columns}
      formFields={formFields}
      transformData={transformData}
      accessToken={accessToken}
    />
  );
};

export default UnitsAdmin;