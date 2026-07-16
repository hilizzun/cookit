import ReferenceTable from './ReferenceTable';
import { useAuth } from '../../contexts/AuthContext';

const DishTypesAdmin = () => {
  const { accessToken } = useAuth();

  const columns = [
    { key: 'id', label: 'ID' },
    { key: 'name', label: 'Название' },
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
    { name: 'name', label: 'Название типа блюда', type: 'text', required: true },
  ];

  const transformData = (data) => ({
    name: data.name || '',
  });

  return (
    <ReferenceTable
      title="Типы блюд"
      endpoint="dishTypes"
      columns={columns}
      formFields={formFields}
      transformData={transformData}
      accessToken={accessToken}
    />
  );
};

export default DishTypesAdmin;