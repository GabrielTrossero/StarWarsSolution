````markdown
# 🎬 MoviesApp API

API para manejo de películas y usuarios, con autenticación JWT. Esta guía explica cómo ejecutar y probar la aplicación.

---

## 🔎 Probar la API

La API está desplegada en un servidor online, aunque también se puede descargar localmente y levantar con dotnet o docker.

Una vez en Swagger podés probar los endpoints. Ejemplos:

* **Listar películas:**
  `GET /api/movies`

* **Registrar usuario:**
  `POST /api/users/register`

* **Login de usuario (obtención de token JWT):**
  `POST /api/auth/login`
  → copiar el token generado y pegarlo en **Authorize** de Swagger (Bearer `<token>`).

* **Acceder a endpoints protegidos (requieren login):**
  `GET /api/users/me`

---

## 📥 Probar la aplicación en el servidor (Render)

La API está desplegada y accesible en:
```
https://starwarssolution.onrender.com/swagger/index.html
```
Tener en cuenta, que al acceder por primera vez, la misma tarda unos segundos en levantarse.
---

## 📥 Clonar el repositorio

Primero, descargar el código desde GitHub:

```bash
git clone https://github.com/<tu-usuario>/<tu-repo>.git
cd <tu-repo>
````

---

## 🛠️ Requisitos previos

Para ejecutar la aplicación necesitás tener instalado:

* [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
* [Docker Desktop](https://www.docker.com/products/docker-desktop) (opcional, si querés correrlo en contenedor)

---

## ▶️ Opción A: Ejecutar localmente con .NET

1. Entrar a la carpeta del proyecto principal (API):

```bash
cd MoviesApp.API
```

2. Restaurar dependencias:

```bash
dotnet restore
```

3. Ejecutar la aplicación:

```bash
dotnet run
```

4. Abrir Swagger en el navegador:
```
http://localhost:5270/swagger/index.html
```

---

## 🐳 Opción B: Ejecutar con Docker localmente

1. Desde la raíz del proyecto (donde está el `Dockerfile`), construir la imagen:

```bash
docker build -t moviesapp .
```

2. Levantar un contenedor exponiendo el puerto 8080:

```bash
docker run -p 8080:8080 moviesapp
```

3. Abrir Swagger en el navegador:

```
http://localhost:8080/swagger/index.html
```

---

## ✅ Notas importantes

* La app usa base de datos en memoria (`InMemoryDatabase`), por lo que los datos se borran al reiniciar.
* No se requiere configuración extra de SQL Server para correr este challenge.
* Swagger está siempre habilitado, por lo que la documentación interactiva de la API está en `/swagger`.

```