import { useState } from 'react';
import { useAuth } from "../../contexts/AuthContext";
import DishTypesAdmin from './DishTypesAdmin';
import EquipmentsAdmin from './EquipmentsAdmin';
import IngredientsAdmin from './IngredientsAdmin';
import UnitsAdmin from './UnitsAdmin';
import RecipeModerationList from './RecipeModerationList';
import ComplaintModerationList from './ComplaintModerationList';
import { Tabs, TabsList, TabsTrigger, TabsContent } from '../ui/tabs';
import { Card, CardContent } from '../ui/card';
import { FaClipboardCheck, FaDatabase} from "react-icons/fa";

export default function AdminPanel() {
  const { user } = useAuth();
  const [mainTab, setMainTab] = useState('moderation');
  const [moderationSubTab, setModerationSubTab] = useState('recipes');

  const isAdmin = user?.roles?.includes('Admin') || user?.roles?.includes('Moderator');

  if (!isAdmin) {
    return (
      <div className="text-center py-12">
        <h2 className="text-2xl font-bold text-destructive mb-4">Доступ запрещен</h2>
        <p className="text-muted-foreground">У вас нет прав для доступа к этой панели</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-3xl font-bold tracking-tight">Админ-панель</h2>
        <p className="text-muted-foreground mt-2">
          {mainTab === 'moderation' && 'Модерация контента'}
          {mainTab === 'directories' && 'Управление справочниками'}
        </p>
      </div>

      <Tabs value={mainTab} onValueChange={setMainTab} className="space-y-4">
        <TabsList>
          <TabsTrigger value="moderation" className="flex items-center gap-2">
            <FaClipboardCheck />
            Модерация
          </TabsTrigger>
          <TabsTrigger value="directories" className="flex items-center gap-2">
            <FaDatabase />
            Справочники
          </TabsTrigger>
        </TabsList>

        <TabsContent value="moderation" className="space-y-4">
          <Card>
            <CardContent className="p-6">
              <Tabs value={moderationSubTab} onValueChange={setModerationSubTab}>
                <TabsList className="mb-4">
                  <TabsTrigger value="recipes" className="flex items-center gap-2">
                    Рецепты
                  </TabsTrigger>
                  <TabsTrigger value="complaints" className="flex items-center gap-2">
                    Жалобы
                  </TabsTrigger>
                </TabsList>
                <TabsContent value="recipes">
                  <RecipeModerationList />
                </TabsContent>
                <TabsContent value="complaints">
                  <ComplaintModerationList />
                </TabsContent>
              </Tabs>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="directories" className="space-y-4">
          <Card>
            <CardContent className="p-6">
              <Tabs defaultValue="dishTypes">
                <TabsList className="mb-4">
                  <TabsTrigger value="dishTypes">Типы блюд</TabsTrigger>
                  <TabsTrigger value="equipments">Оборудование</TabsTrigger>
                  <TabsTrigger value="ingredients">Ингредиенты</TabsTrigger>
                  <TabsTrigger value="units">Единицы измерения</TabsTrigger>
                </TabsList>
                <TabsContent value="dishTypes">
                  <DishTypesAdmin />
                </TabsContent>
                <TabsContent value="equipments">
                  <EquipmentsAdmin />
                </TabsContent>
                <TabsContent value="ingredients">
                  <IngredientsAdmin />
                </TabsContent>
                <TabsContent value="units">
                  <UnitsAdmin />
                </TabsContent>
              </Tabs>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}