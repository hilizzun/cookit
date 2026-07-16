import React from 'react';
import { Document, Page, Text, View, StyleSheet, Font, Image } from '@react-pdf/renderer';

Font.register({
  family: 'Noto Sans',
  fonts: [
    { src: '/fonts/NotoSans-Regular.ttf', fontWeight: 'normal' },
    { src: '/fonts/NotoSans-Bold.ttf', fontWeight: 'bold' }
  ]
});

const styles = StyleSheet.create({
  page: { padding: 30, fontFamily: 'Noto Sans', fontSize: 12, color: '#1f2937' },
  title: { fontSize: 24, marginBottom: 20, textAlign: 'center', fontWeight: 'bold', color: '#111827' },
  sectionTitle: {
    fontSize: 16,
    marginBottom: 12,
    fontWeight: 'bold',
    backgroundColor: '#f3f4f6',
    padding: 6,
    borderRadius: 4,
    color: '#1f2937',
  },
  table: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    marginTop: 8,
  },
  tableRow: {
    flexDirection: 'row',
    width: '100%',
    borderBottomWidth: 1,
    borderBottomColor: '#e5e7eb',
    borderBottomStyle: 'solid',
    paddingVertical: 4,
  },
  tableCellName: {
    width: '50%',
    fontSize: 11,
    paddingRight: 8,
  },
  tableCellQuantity: {
    width: '35%',
    fontSize: 11,
    textAlign: 'right',
    fontFamily: 'Noto Sans',
  },
  tableCellCheckbox: {
    width: '15%',
    fontSize: 14,
    textAlign: 'right',
    paddingRight: 4,
  },
  headerCellName: {
    width: '50%',
    fontSize: 12,
    fontWeight: 'bold',
    color: '#374151',
  },
  headerCellQuantity: {
    width: '35%',
    fontSize: 12,
    fontWeight: 'bold',
    textAlign: 'right',
    color: '#374151',
  },
  headerCellCheckbox: {
    width: '15%',
    fontSize: 12,
    fontWeight: 'bold',
    textAlign: 'right',
    color: '#374151',
  },
  headerRow: {
    flexDirection: 'row',
    width: '100%',
    borderBottomWidth: 2,
    borderBottomColor: '#d1d5db',
    marginBottom: 4,
    paddingBottom: 4,
  },
});

const ShoppingListPDF = ({ groupedIngredients }) => {
  const sortedIngredients = [...groupedIngredients].sort((a, b) => a.name.localeCompare(b.name));

  return (
    <Document>
      <Page size="A4" style={styles.page}>
        <View style={{ flexDirection: 'row', alignItems: 'center', marginBottom: 20, justifyContent: 'space-between' }}>
          <Text style={[styles.title, { flex: 1, textAlign: 'center', marginBottom: 0 }]}>Список покупок</Text>
          <Image src="/logo.png" style={{ width: 70, height: 50 }} />
        </View>

        {groupedIngredients.length > 0 ? (
          <View>
        
            <View style={styles.headerRow}>
              <Text style={styles.headerCellName}>Ингредиент</Text>
              <Text style={styles.headerCellQuantity}>Количество</Text>
              <Text style={styles.headerCellCheckbox}>✓</Text>
            </View>

            <View style={styles.table}>
              {sortedIngredients.map((ing, idx) => (
                <View key={idx} style={styles.tableRow}>
                  <Text style={styles.tableCellName}>{ing.name}</Text>
                  <Text style={styles.tableCellQuantity}>
                    {Math.ceil(ing.quantity)} {ing.unit}
                  </Text>
                  <Text style={styles.tableCellCheckbox}>[  ]</Text>
                </View>
              ))}
            </View>
          </View>
        ) : (
          <Text style={styles.sectionTitle}>Нет активных ингредиентов для отображения</Text>
        )}
      </Page>
    </Document>
  );
};

export default ShoppingListPDF;