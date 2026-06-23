# HU - Galeria de producto e imagen referencial de inventario

## Historia de usuario

Como administrador de imagenes,
quiero asociar productos a categorias y cargar varias imagenes ordenadas por producto,
para que el front pueda mostrar un catalogo tipo e-commerce filtrable por categoria, con cards que permitan ver varias perspectivas del producto.

Como cliente o usuario del front,
quiero ver todos los productos o filtrarlos por categoria,
para explorar los productos disponibles en Impresion 3D, Papeleria Creativa y Sublimacion de forma clara y visual.

Como administrador de inventario,
quiero cargar una sola imagen referencial al crear o editar un item,
para identificarlo internamente sin afectar la galeria publica del catalogo.

## Alcance funcional

### Admin de imagenes

- Seleccionar una categoria.
- Ver productos asociados a esa categoria.
- Asociar un producto existente a la categoria cuando aplique.
- Cargar una o varias imagenes para el producto seleccionado.
- Ordenar las imagenes en el orden en que se visualizaran en el front.
- Definir una imagen principal.
- Eliminar imagenes.
- Invalidar Redis para que el catalogo refleje los cambios.

### Front publico

- Mostrar todos los productos.
- Permitir filtrar por categoria.
- Mostrar productos asociados a Impresion 3D, Papeleria Creativa y Sublimacion.
- Mostrar en el front solo productos que tengan al menos una imagen asociada.
- Mostrar la imagen principal por defecto.
- En desktop, rotar imagenes al pasar el mouse sobre la card con un intervalo comodo, sin cambios demasiado rapidos.
- En mobile, permitir carrusel/swipe dentro de la card.
- Al hacer clic en la imagen/card, abrir un modal de visualizacion del producto.
- En el modal, permitir navegar las imagenes con flechas hacia adelante y hacia atras.
- En el modal, permitir navegar con teclado usando flecha izquierda y flecha derecha.
- En el modal, mantener indicadores de posicion cuando el producto tenga varias imagenes.
- Mostrar placeholder si el producto no tiene imagenes.

### Inventario

- Permitir cargar una sola imagen referencial al crear o editar un item.
- Permitir reemplazar o eliminar la imagen.
- La imagen de inventario no participa en el catalogo publico.
- La imagen de inventario no pertenece al modulo de administracion de imagenes.

## Modelo de datos

Se mantiene `Products.CategoryId` como categoria principal para compatibilidad con los flujos existentes, pero se agrega `ProductCategories` como relacion muchos-a-muchos para soportar productos en varias categorias.

Tablas principales:

- `core.products`
- `core.categories`
- `core.product_categories`
- `core.product_images`
- `core.inventory_items`

Campos clave:

- `ProductCategories.ProductId`
- `ProductCategories.CategoryId`
- `ProductCategories.SortOrder`
- `ProductImages.ProductId`
- `ProductImages.SortOrder`
- `ProductImages.IsPrimary`
- `InventoryItems.ImageUrl`
- `InventoryItems.ImageStoragePath`

## Endpoints API

Productos por categoria:

```http
GET /api/v1/categories/{categoryId}/products
POST /api/v1/categories/{categoryId}/products/{productId}
PUT /api/v1/categories/{categoryId}/products/reorder
DELETE /api/v1/categories/{categoryId}/products/{productId}
```

Galeria de producto:

```http
GET /api/v1/products/{productId}/images
POST /api/v1/products/{productId}/images
PUT /api/v1/products/{productId}/images/reorder
PUT /api/v1/products/{productId}/images/{imageId}/primary
DELETE /api/v1/products/{productId}/images/{imageId}
```

Catalogo publico:

```http
GET /api/v1/catalogs/products
GET /api/v1/catalogs/products?categoryId={categoryId}
```

Imagen unica de inventario:

```http
PUT /api/v1/inventory/items/{itemId}/image
DELETE /api/v1/inventory/items/{itemId}/image
```

## Redis

Claves relacionadas:

- `amas:catalogs:products:all`
- `amas:catalogs:products:category:{categoryId}`
- `amas:categories:{categoryId}:products`
- `amas:products:{productId}:images`

Se invalida cache cuando:

- Se asocia o desasocia producto a categoria.
- Se reordenan productos de una categoria.
- Se cargan imagenes.
- Se eliminan imagenes.
- Se cambia imagen principal.
- Se reordenan imagenes.
- Se cambia imagen de inventario.

## Criterios de aceptacion

1. Un producto puede estar asociado a una o varias categorias.
2. El admin puede seleccionar categoria y ver productos asociados.
3. El admin puede asociar productos existentes a una categoria.
4. El admin puede cargar multiples imagenes para un producto.
5. El admin puede ordenar imagenes.
6. El admin puede marcar imagen principal.
7. El admin puede eliminar imagenes.
8. El catalogo publico consulta todos los productos.
9. El catalogo publico consulta productos por categoria.
10. El catalogo publico no muestra productos sin imagenes asociadas.
11. La card usa imagen principal por defecto.
12. En desktop, la card rota imagenes al pasar el mouse con un intervalo aproximado entre 1.5 y 2 segundos.
13. En mobile, la card permite carrusel/swipe.
14. Al hacer clic sobre la imagen/card, se abre un modal con la imagen activa.
15. El modal permite avanzar y retroceder imagenes con flechas visibles.
16. El modal permite navegar con teclado usando flecha izquierda y flecha derecha.
17. El orden de imagenes respeta `SortOrder`.
18. Redis cachea catalogo e imagenes.
19. Redis se invalida correctamente ante cambios.
20. Inventario permite una sola imagen referencial al crear o editar item.
21. La imagen de inventario no se usa en catalogo publico.
22. El storage queda configurable para moverlo luego a VPS o nube.
