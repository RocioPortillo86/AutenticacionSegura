# SoporteTecnico-Rag
Asistente de soporte técnico.

El sistema propuesto es una aplicación de Punto de Venta diseñada para operar en entornos comerciales pequeños o medianos. Su objetivo principal es permitir el registro seguro de usuarios y roles, el control de inventario de materiales o productos y la operación de la caja registradora, asegurando trazabilidad de las ventas y reportes básicos para el administrador.
El sistema estará desarrollado en Visual Studio 2022 con C# y ASP.NET Framework 4.8 y SQL Server 2019 como base de datos. Para la autenticación y autorización se utilizará ASP.NET Identity 2, que permitirá diferenciar entre el rol de Administrador, encargado de crear usuarios y administrar el catálogo de materiales, y el rol de Cajero, responsable de registrar las ventas


Requisitos de Instalacion para correr aplicacipon
-  Visual Studio 2022, .NET Framework 4.8, SQL Server Express una vez instalado descargar el paquete BCrypt.Net-Next
-  El proyecto del aplicativo lo guardare en la raiz de este repositorio se llama PuntoVentas
-  Para ingresar al Sistema se utiliza de usuario admin@example.com Pass: Admin123


En este rpositorio se ejecuta un promp de genera los siguientes archivos:
- EspecificacionesProyecto.md
    El cual incluye la configuración del entorno, Estructura Inicial del proyecto, Desarrollo Backend Core y FrintEnd
- Codigo.md
    Recomendación de código básico den C# de las funciones del aplicativo

- analisis_ventas.txt.md
    Muestra una analisis de ventas con valores
  
- ventas_por_producto.png
    Grafico de Ventas
