import { Document, Page, Text, View, StyleSheet, Font, Image } from '@react-pdf/renderer';

Font.register({
  family: 'Noto Sans',
  fonts: [
    { src: '/fonts/NotoSans-Regular.ttf', fontWeight: 'normal' },
    { src: '/fonts/NotoSans-Bold.ttf', fontWeight: 'bold' }
  ]
});

const formatDescription = (html) => {
  if (!html) return '';
  let text = html
    .replace(/<br\s*\/?>/gi, '\n')
    .replace(/<\/(p|div|li|h[1-6])>/gi, '\n\n')
    .replace(/<[^>]*>/g, '')
    .replace(/\n\s*\n/g, '\n\n')
    .replace(/&nbsp;/g, ' ')
    .replace(/&quot;/g, '"')
    .replace(/&amp;/g, '&')
    .replace(/&lt;/g, '<')
    .replace(/&gt;/g, '>');
  return text;
};

const StarRating = ({ level, max = 5, filledColor = '#eab308', emptyColor = '#d1d5db' }) => {
  const filledStars = '*'.repeat(level);
  const emptyStars = '*'.repeat(max - level);
  return (
    <Text>
      <Text style={{ fontSize: 14, fontWeight: 'bold', color: filledColor }}>{filledStars}</Text>
      <Text style={{ fontSize: 14, fontWeight: 'bold', color: emptyColor }}>{emptyStars}</Text>
    </Text>
  );
};

const styles = StyleSheet.create({
  page: { padding: 30, fontFamily: 'Noto Sans', fontSize: 12, color: '#1f2937' },
  title: { fontSize: 24, marginBottom: 20, textAlign: 'center', fontWeight: 'bold', color: '#111827' },
  section: { marginBottom: 20 },
  sectionTitle: {
    fontSize: 16,
    marginBottom: 8,
    fontWeight: 'bold',
    backgroundColor: '#f3f4f6',
    padding: 6,
    borderRadius: 4,         
    color: '#1f2937',
  },
  row: { flexDirection: 'row', marginBottom: 8, gap: 12 },
  badge: { backgroundColor: '#f9fafb', padding: 8, borderRadius: 8, marginBottom: 8 },
  badgeText: { fontSize: 10, color: '#4b5563', marginBottom: 2 },
  badgeValue: { fontSize: 14, fontWeight: 'bold', color: '#111827' },
  list: { marginLeft: 12 },
  listItem: { marginBottom: 4 },
  nutritionRow: { flexDirection: 'row', justifyContent: 'space-between', gap: 8 },
  nutritionCard: { flex: 1, backgroundColor: '#f5f3ff', padding: 8, borderRadius: 8, alignItems: 'center' },
  nutritionValue: { fontSize: 16, fontWeight: 'bold', color: '#6b21a5' },
  nutritionLabel: { fontSize: 10, color: '#4b5563' },
  ratingContainer: { flexDirection: 'row', alignItems: 'center', marginTop: 4 },
  ratingText: { fontSize: 12, marginLeft: 4, color: '#4b5563' },
});

const NutritionBlock = ({ title, calories, proteins, fats, carbs }) => (
  <View style={{ marginBottom: 12 }}>
    <Text style={{ fontSize: 12, fontWeight: 'bold', marginBottom: 4 }}>{title}</Text>
    <View style={styles.nutritionRow}>
      <View style={styles.nutritionCard}><Text style={styles.nutritionValue}>{calories.toFixed(1)}</Text><Text style={styles.nutritionLabel}>ккал</Text></View>
      <View style={[styles.nutritionCard, { backgroundColor: '#eff6ff' }]}><Text style={[styles.nutritionValue, { color: '#1e40af' }]}>{proteins.toFixed(1)}</Text><Text style={styles.nutritionLabel}>белки</Text></View>
      <View style={[styles.nutritionCard, { backgroundColor: '#fef9c3' }]}><Text style={[styles.nutritionValue, { color: '#a16207' }]}>{fats.toFixed(1)}</Text><Text style={styles.nutritionLabel}>жиры</Text></View>
      <View style={[styles.nutritionCard, { backgroundColor: '#f0fdf4' }]}><Text style={[styles.nutritionValue, { color: '#166534' }]}>{carbs.toFixed(1)}</Text><Text style={styles.nutritionLabel}>углеводы</Text></View>
    </View>
  </View>
);

const RecipePDF = ({ recipe }) => (
  <Document>
    <Page size="A4" style={styles.page}>
    <View style={{ flexDirection: 'row', alignItems: 'center', marginBottom: 20, justifyContent: 'space-between' }}>
    <Text style={[styles.title, { flex: 1, textAlign: 'center', marginBottom: 0 }]}>{recipe.name}</Text>
    <Image src="/logo.png" style={{ width: 70, height: 50 }} />
    </View>

      <View style={styles.row}>
        <View style={styles.badge}>
          <Text style={styles.badgeText}>Время</Text>
          <Text style={styles.badgeValue}>{recipe.cookingTimeWithoutUser} / {recipe.cookingTimeWithUser} мин</Text>
        </View>
        <View style={styles.badge}>
          <Text style={styles.badgeText}>Сложность</Text>
          <StarRating level={recipe.difficultyLevel} filledColor="#10b981" emptyColor="#d1d5db" />
        </View>
        <View style={styles.badge}>
          <Text style={styles.badgeText}>Острота</Text>
          <StarRating level={recipe.spicinessLevel} filledColor="#ef4444" emptyColor="#d1d5db" />
        </View>
        <View style={styles.badge}>
          <Text style={styles.badgeText}>Тип блюда</Text>
          <Text style={styles.badgeValue}>{recipe.dishType?.name || '—'}</Text>
        </View>
        <View style={styles.badge}>
          <Text style={styles.badgeText}>Порций</Text>
          <Text style={styles.badgeValue}>{recipe.servings}</Text>
        </View>
      </View>

      <View style={styles.section}>
        <Text style={styles.sectionTitle}>Рейтинг</Text>
        <View style={styles.ratingContainer}>
          <StarRating level={Math.floor(recipe.averageRating || 0)} filledColor="#eab308" emptyColor="#d1d5db" />
          <Text style={styles.ratingText}>
            {recipe.averageRating?.toFixed(1)} ({recipe.totalRatings || 0} оценок)
          </Text>
        </View>
      </View>

      {recipe.recipeEquipments?.length > 0 && (
        <View wrap={false} style={styles.section}>
          <Text style={styles.sectionTitle}>Оборудование</Text>
          <View style={styles.list}>
            {recipe.recipeEquipments.map((eq, idx) => (
              <Text key={idx} style={styles.listItem}>• {eq.equipmentName}</Text>
            ))}
          </View>
        </View>
      )}

      {recipe.recipeIngredients?.length > 0 && (
        <View wrap={false} style={styles.section}>
          <Text style={styles.sectionTitle}>Ингредиенты</Text>
          <View style={styles.list}>
            {recipe.recipeIngredients.map((ing, idx) => (
              <Text key={idx} style={styles.listItem}>
                • {ing.ingredientName}{ing.quantity ? ` — ${ing.quantity} ${ing.unitName || ''}` : ''}
              </Text>
            ))}
          </View>
        </View>
      )}

      <View style={styles.section}>
        <View wrap={false}>
            <Text style={styles.sectionTitle}>Пищевая ценность</Text>
            <NutritionBlock title="На всё блюдо" calories={recipe.totalCalories || 0} proteins={recipe.totalProteins || 0} fats={recipe.totalFats || 0} carbs={recipe.totalCarbohydrates || 0} />
        </View>
        <View wrap={false}>
            <NutritionBlock title="На порцию" calories={recipe.caloriesPerServing || 0} proteins={recipe.proteinsPerServing || 0} fats={recipe.fatsPerServing || 0} carbs={recipe.carbohydratesPerServing || 0} />
        </View>
        <View wrap={false}>
            <NutritionBlock title="На 100 г" calories={recipe.caloriesPer100g || 0} proteins={recipe.proteinsPer100g || 0} fats={recipe.fatsPer100g || 0} carbs={recipe.carbohydratesPer100g || 0} />
        </View>
      </View>

      <View wrap={false} style={styles.section}>
        <Text style={styles.sectionTitle}>Приготовление</Text>
        <Text style={{ whiteSpace: 'pre-wrap' }}>{formatDescription(recipe.fullDescription)}</Text>
      </View>

      <View style={{ marginTop: 20, borderTopWidth: 1, borderTopColor: '#e5e7eb', paddingTop: 12 }}>
        <Text style={{ fontSize: 10, color: '#6b7280', textAlign: 'center' }}>
          Автор: {recipe.creatorUsername || 'Неизвестный'}
        </Text>
      </View>
    </Page>
  </Document>
);

export default RecipePDF;