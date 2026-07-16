import { useState, useEffect } from 'react';
import { Button } from '../ui/button';
import { Input } from '../ui/input';
import { Label } from '../ui/label';
import { Checkbox } from '../ui/checkbox';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '../ui/table';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '../ui/dialog';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '../ui/alert-dialog';
import { ScrollArea } from '../ui/scroll-area';
import { FaEdit, FaTrash, FaUndo, FaPlus } from 'react-icons/fa';
import { API_BASE_URL } from '../../config/settings';

const ReferenceTable = ({ 
  title, 
  endpoint, 
  columns, 
  formFields,
  transformData = (data) => data,
  accessToken,
  onAutoFill,
  autoFillFieldName = 'name',
}) => {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [editingItem, setEditingItem] = useState(null);
  const [formData, setFormData] = useState({});
  const [dialogOpen, setDialogOpen] = useState(false);
  const [autoFillLoading, setAutoFillLoading] = useState(false);

  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [itemToDelete, setItemToDelete] = useState(null);
  const [isDeleting, setIsDeleting] = useState(false);

  const [restoreDialogOpen, setRestoreDialogOpen] = useState(false);
  const [itemToRestore, setItemToRestore] = useState(null);
  const [isRestoring, setIsRestoring] = useState(false);

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      setLoading(true);
      const response = await fetch(`${API_BASE_URL}/api/admin/${endpoint}?includeDeleted=true`, {
        headers: { Authorization: `Bearer ${accessToken}` },
      });
      if (!response.ok) throw new Error('Ошибка загрузки данных');
      const data = await response.json();
      setItems(data);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleInputChange = (e) => {
    const { name, value, type } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: type === 'checkbox' ? e.target.checked : value
    }));
  };

  const handleCheckboxChange = (name, checked) => {
    setFormData(prev => ({ ...prev, [name]: checked }));
  };

  const handleEdit = (item) => {
    setEditingItem(item);
    setFormData(transformData(item));
    setDialogOpen(true);
  };

  const handleAdd = () => {
    setEditingItem(null);
    setFormData({});
    setDialogOpen(true);
  };

  const handleDeleteClick = (id) => {
    setItemToDelete(id);
    setDeleteDialogOpen(true);
  };

  const confirmDelete = async () => {
    setIsDeleting(true);
    try {
      const response = await fetch(`${API_BASE_URL}/api/admin/${endpoint}/${itemToDelete}`, {
        method: 'DELETE',
        headers: { Authorization: `Bearer ${accessToken}` },
      });
      if (!response.ok) throw new Error('Ошибка удаления');
      setDeleteDialogOpen(false);
      setItemToDelete(null);
      fetchData();
    } catch (err) {
      console.error(err);
    } finally {
      setIsDeleting(false);
    }
  };

  const handleRestoreClick = (id) => {
    setItemToRestore(id);
    setRestoreDialogOpen(true);
  };

  const confirmRestore = async () => {
    setIsRestoring(true);
    try {
      const response = await fetch(`${API_BASE_URL}/api/admin/${endpoint}/${itemToRestore}/restore`, {
        method: 'POST',
        headers: { Authorization: `Bearer ${accessToken}` },
      });
      if (!response.ok) throw new Error('Ошибка восстановления');
      setRestoreDialogOpen(false);
      setItemToRestore(null);
      fetchData();
    } catch (err) {
      console.error(err);
    } finally {
      setIsRestoring(false);
    }
  };

  const handleAutoFill = async () => {
    if (!onAutoFill) return;
    const nameValue = formData[autoFillFieldName]?.trim();
    if (!nameValue) {
      alert('Сначала введите название');
      return;
    }
    setAutoFillLoading(true);
    try {
      const nutrition = await onAutoFill(nameValue);
      setFormData(prev => ({ ...prev, ...nutrition }));
    } catch (err) {
      console.error(err);
      alert('Не удалось получить КБЖУ для этого ингредиента');
    } finally {
      setAutoFillLoading(false);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      const url = editingItem
        ? `${API_BASE_URL}/api/admin/${endpoint}/${editingItem.id}`
        : `${API_BASE_URL}/api/admin/${endpoint}`;
      const method = editingItem ? 'PUT' : 'POST';
      const dataToSend = { ...formData };
      if (editingItem) dataToSend.id = editingItem.id;

      const response = await fetch(url, {
        method,
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${accessToken}`,
        },
        body: JSON.stringify(dataToSend),
      });
      if (!response.ok) throw new Error('Ошибка сохранения');
      setDialogOpen(false);
      setEditingItem(null);
      setFormData({});
      fetchData();
    } catch (err) {
      console.error(err);
    }
  };

  if (loading) {
    return (
      <div className="flex justify-center py-8">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <div className="flex justify-between items-center">
        <h3 className="text-lg font-semibold">{title}</h3>
        <Button onClick={handleAdd} size="sm">
          <FaPlus className="mr-2 h-4 w-4" /> Добавить
        </Button>
      </div>

      <div className="border rounded-lg overflow-hidden">
        <ScrollArea className="h-[400px]">
          <Table>
            <TableHeader>
              <TableRow>
                {columns.map((col) => (
                  <TableHead key={col.key}>{col.label}</TableHead>
                ))}
                <TableHead className="text-right">Действия</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {items.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={columns.length + 1} className="text-center py-8 text-muted-foreground">
                    Нет данных для отображения
                  </TableCell>
                </TableRow>
              ) : (
                items.map((item) => (
                  <TableRow key={item.id} className={item.isDeleted ? 'bg-muted/50' : ''}>
                    {columns.map((col) => (
                      <TableCell key={col.key}>
                        {col.render ? col.render(item[col.key], item) : item[col.key]}
                      </TableCell>
                    ))}
                    <TableCell className="text-right">
                      {item.isDeleted ? (
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleRestoreClick(item.id)}
                          className="text-green-600 hover:text-green-700"
                        >
                          <FaUndo className="h-4 w-4" />
                        </Button>
                      ) : (
                        <div className="flex justify-end gap-2">
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => handleEdit(item)}
                            disabled={item.isUsedInRecipes}
                            className={item.isUsedInRecipes ? 'opacity-50 cursor-not-allowed' : ''}
                          >
                            <FaEdit className="h-4 w-4" />
                          </Button>
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => handleDeleteClick(item.id)}
                            className="text-destructive hover:text-destructive"
                          >
                            <FaTrash className="h-4 w-4" />
                          </Button>
                        </div>
                      )}
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </ScrollArea>
      </div>

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="sm:max-w-md bg-white">
          <DialogHeader>
            <DialogTitle>{editingItem ? 'Редактировать' : 'Добавить'}</DialogTitle>
          </DialogHeader>
          <form onSubmit={handleSubmit} className="space-y-4">
            {formFields.map((field) => {
              const isAutoFillField = onAutoFill && field.name === autoFillFieldName;
              return (
                <div key={field.name} className="space-y-2">
                  <Label htmlFor={field.name}>{field.label}</Label>
                  <div className="flex gap-2">
                    {field.type === 'checkbox' ? (
                      <div className="flex items-center space-x-2">
                        <Checkbox
                          id={field.name}
                          checked={formData[field.name] || false}
                          onCheckedChange={(checked) => handleCheckboxChange(field.name, checked)}
                        />
                        <Label htmlFor={field.name}>Да</Label>
                      </div>
                    ) : (
                      <Input
                        id={field.name}
                        name={field.name}
                        type={field.type || 'text'}
                        value={formData[field.name] || ''}
                        onChange={handleInputChange}
                        step={field.step}
                        required={field.required}
                        className="flex-1"
                      />
                    )}
                    {isAutoFillField && (
                      <Button
                        type="button"
                        variant="outline"
                        onClick={handleAutoFill}
                        disabled={autoFillLoading}
                        className="whitespace-nowrap"
                      >
                        {autoFillLoading ? 'Загрузка...' : 'Заполнить КБЖУ'}
                      </Button>
                    )}
                  </div>
                </div>
              );
            })}
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => setDialogOpen(false)}>
                Отмена
              </Button>
              <Button type="submit" variant="violet">
                Сохранить
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>

      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent className="bg-white">
          <AlertDialogHeader>
            <AlertDialogTitle>Подтверждение удаления</AlertDialogTitle>
            <AlertDialogDescription>
              Вы уверены, что хотите удалить этот элемент? Это действие можно отменить позже.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel onClick={() => setItemToDelete(null)}>Отмена</AlertDialogCancel>
            <AlertDialogAction onClick={confirmDelete} className="bg-destructive text-destructive-foreground hover:bg-destructive/90">
              {isDeleting ? 'Удаление...' : 'Удалить'}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      <AlertDialog open={restoreDialogOpen} onOpenChange={setRestoreDialogOpen}>
        <AlertDialogContent className="bg-white">
          <AlertDialogHeader>
            <AlertDialogTitle>Подтверждение восстановления</AlertDialogTitle>
            <AlertDialogDescription>
              Вы уверены, что хотите восстановить этот элемент?
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel onClick={() => setItemToRestore(null)}>Отмена</AlertDialogCancel>
            <AlertDialogAction onClick={confirmRestore} className="bg-green-600 text-white hover:bg-green-700">
              {isRestoring ? 'Восстановление...' : 'Восстановить'}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
};

export default ReferenceTable;