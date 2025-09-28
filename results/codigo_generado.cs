✅ API Key configurada correctamente

🤖 Modelo listo

Para configurar un entorno de desarrollo para un proyecto de Punto de Venta en Visual Studio 2022 utilizando C# y ASP.NET Framework 4.8, sigue estas instrucciones claras y concisas:

### Requisitos Previos

1. **Instalar Visual Studio 2022**:
   - Descarga e instala Visual Studio 2022 desde el [sitio oficial de Microsoft](https://visualstudio.microsoft.com/).
   - Durante la instalación, selecciona la carga de trabajo "Desarrollo web" y asegúrate de incluir el **.NET Framework 4.8**.

2. **Instalar SQL Server Express**:
   - Descarga e instala SQL Server Express desde el [sitio oficial de Microsoft](https://www.microsoft.com/en-us/sql-server/sql-server-downloads).
   - Asegúrate de que la opción de **LocalDB** esté seleccionada durante la instalación.

### Creación del Proyecto

1. **Crear un nuevo proyecto**:
   - Abre Visual Studio 2022.
   - Selecciona "Crear un nuevo proyecto".
   - En el cuadro de búsqueda, escribe "ASP.NET Web Application (.NET Framework)" y selecciona esta opción.
   - Haz clic en "Siguiente".

2. **Configurar el proyecto**:
   - Asigna un nombre a tu proyecto, por ejemplo, `PuntoDeVenta`.
   - Selecciona la ubicación donde deseas guardar el proyecto.
   - Asegúrate de que la opción "Crear un repositorio Git" esté marcada si deseas usar control de versiones.
   - Haz clic en "Crear".

3. **Seleccionar plantilla de proyecto**:
   - En la ventana "Crear una nueva aplicación web", selecciona "Aplicación web (Modelo-Vista-Controlador)".
   - Marca la opción "Autenticación" y selecciona "Cuentas de usuario individuales".
   - Haz clic en "Crear".

### Configuración de la Cadena de Conexión a LocalDB

1. **Modificar el archivo `Web.config`**:
   - Abre el archivo `Web.config` en la raíz del proyecto.
   - Busca la sección `<connectionStrings>` y modifica la cadena de conexión para que apunte a LocalDB. Debería verse algo así:

   ```xml
   <connectionStrings>
       <add name="DefaultConnection" 
            connectionString="Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=PuntoDeVenta;Integrated Security=True;MultipleActiveResultSets=True" 
            providerName="System.Data.SqlClient" />
   </connectionStrings>
   ```

2. **Crear la base de datos**:
   - Abre la **Consola del Administrador de Paquetes** (Tools > NuGet Package Manager > Package Manager Console).
   - Ejecuta el siguiente comando para aplicar las migraciones y crear la base de datos:

   ```powershell
   Update-Database
   ```

### Resumen

Con estos pasos, has configurado un entorno de desarrollo para un proyecto de Punto de Venta en Visual Studio 2022 utilizando C# y ASP.NET Framework 4.8. Has creado un nuevo proyecto con autenticación de cuentas de usuario individuales y configurado la cadena de conexión a LocalDB para la base de datos. Asegúrate de seguir las mejores prácticas de desarrollo y seguridad a medida que avanzas en el desarrollo de tu aplicación.

📂 Working dir: /home/runner/work/PuntoVentas/PuntoVentas
✅ Guardado en: /home/runner/work/PuntoVentas/PuntoVentas/result/ConfiguracionEntorno.md
📏 Tamaño (bytes): 2932
🧪 Existe?: True
