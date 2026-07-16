import ReferenceTable from './ReferenceTable';
import { useAuth } from '../../contexts/AuthContext';

const EquipmentsAdmin = () => {
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
    { name: 'name', label: 'Название оборудования', type: 'text', required: true },
  ];

  const transformData = (data) => ({
    name: data.name || '',
  });

  return (
    <ReferenceTable
      title="Оборудование"
      endpoint="equipments"
      columns={columns}
      formFields={formFields}
      transformData={transformData}
      accessToken={accessToken}
    />
  );
};

export default EquipmentsAdmin;