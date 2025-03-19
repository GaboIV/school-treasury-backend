# School Treasury Backend

## Ejecutar con Docker

### Requisitos previos
- Docker
- Docker Compose

### Pasos para ejecutar la aplicación

1. Clonar el repositorio:
```bash
git clone <url-del-repositorio>
cd school-treasury-backend
```

2. Construir y ejecutar los contenedores:
```bash
docker-compose up -d
```

3. La aplicación estará disponible en:
   - API: http://localhost:5200
   - Swagger: http://localhost:5200/swagger
   - Seq (Logs): http://localhost:8081

### Servicios incluidos
- API .NET 9.0
- MongoDB
- Seq (para visualización de logs)

### Detener la aplicación
```bash
docker-compose down
```

Para eliminar también los volúmenes (datos persistentes):
```bash
docker-compose down -v
``` 