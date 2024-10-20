import cx from 'classnames'
import { Option } from '@willow/ui'
import styles from './CategoryOption.css'

export default function CategoryOption({ category, level = 0 }) {
  return (
    <>
      <Option
        value={category}
        className={(option) =>
          cx({
            [styles.selected]: option.isSelected,
            [styles.subCategory]: level > 0,
          })
        }
      >
        <span className={styles.text} style={{ paddingLeft: level * 16 }}>
          {category.name}
        </span>
      </Option>
      {category.childCategories.map((childCategory) => (
        <CategoryOption
          key={childCategory.id}
          category={childCategory}
          level={level + 1}
        />
      ))}
    </>
  )
}
