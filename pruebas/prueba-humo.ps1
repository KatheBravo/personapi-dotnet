<#
  Pruebas de humo de personapi-dotnet
  -----------------------------------
  Recorre el CRUD completo de las cuatro tablas, tanto por la API REST como por
  las vistas web (formularios MVC), y al final dice cuántas pruebas pasaron.

  Uso (con la aplicación ya corriendo, por ejemplo con INICIAR.bat):
      powershell -ExecutionPolicy Bypass -File pruebas\prueba-humo.ps1
      powershell -ExecutionPolicy Bypass -File pruebas\prueba-humo.ps1 -BaseUrl http://localhost:5000

  Usa datos de prueba propios (cédulas 1999999901 y 1999999902, profesión 900,
  teléfonos 3001112233 y 3001112244) y los borra al terminar, así que no daña
  los datos de ejemplo.
#>
param([string]$BaseUrl = "http://localhost:8080")

$ErrorActionPreference = "Stop"
$script:ok = 0
$script:fallos = 0

# Hace una petición HTTP y devuelve el código de estado y el cuerpo.
# Usamos HttpClient (y no Invoke-WebRequest) porque así podemos ver tal cual las
# respuestas 302 de los formularios y los errores 4xx sin que PowerShell las convierta en excepciones.
Add-Type -AssemblyName System.Net.Http
function Nuevo-Cliente {
    $manejador = New-Object System.Net.Http.HttpClientHandler
    $manejador.AllowAutoRedirect = $false
    $manejador.CookieContainer = New-Object System.Net.CookieContainer
    return New-Object System.Net.Http.HttpClient($manejador)
}
$script:clienteApi = Nuevo-Cliente

function Pedir {
    param([string]$Metodo, [string]$Ruta, $Json = $null, [string]$Formulario = $null, $Sesion = $null)
    $cliente = if ($Sesion) { $Sesion } else { $script:clienteApi }
    $mensaje = New-Object System.Net.Http.HttpRequestMessage([System.Net.Http.HttpMethod]::new($Metodo), "$BaseUrl$Ruta")
    if ($null -ne $Json) {
        $mensaje.Content = New-Object System.Net.Http.StringContent(($Json | ConvertTo-Json -Compress), [Text.Encoding]::UTF8, "application/json")
    }
    if ($Formulario -or $Metodo -eq "POST" -and $null -eq $Json) {
        $mensaje.Content = New-Object System.Net.Http.StringContent([string]$Formulario, [Text.Encoding]::UTF8, "application/x-www-form-urlencoded")
    }
    try {
        $resp = $cliente.SendAsync($mensaje).GetAwaiter().GetResult()
        $texto = $resp.Content.ReadAsStringAsync().GetAwaiter().GetResult()
        return @{ Codigo = [int]$resp.StatusCode; Cuerpo = [string]$texto }
    } catch {
        return @{ Codigo = 0; Cuerpo = $_.Exception.Message }
    }
}

# Cuenta los elementos de una respuesta JSON que es una lista.
function Contar([string]$Cuerpo) { return @($Cuerpo | ConvertFrom-Json | ForEach-Object { $_ }).Count }

# Anota una prueba como OK o FALLO y la imprime.
function Revisar([string]$Nombre, [bool]$Paso, [string]$Detalle = "") {
    if ($Paso) { $script:ok++; Write-Output ("[OK]    " + $Nombre) }
    else { $script:fallos++; Write-Output ("[FALLO] " + $Nombre + "  -> " + $Detalle) }
}

function Esperar([string]$Nombre, $Resp, [int]$Codigo) {
    Revisar $Nombre ($Resp.Codigo -eq $Codigo) ("esperaba $Codigo, llegó " + $Resp.Codigo + " " + $Resp.Cuerpo)
}

# Envía un formulario MVC: primero abre la página para obtener el token antifalsificación
# (protección CSRF que ASP.NET Core exige en los POST) y luego hace el POST con ese token.
function EnviarFormulario([string]$RutaGet, [string]$RutaPost, [hashtable]$Campos) {
    $sesion = Nuevo-Cliente
    $pagina = Pedir GET $RutaGet -Sesion $sesion
    $m = [regex]::Match($pagina.Cuerpo, '__RequestVerificationToken"[^>]*value="([^"]+)"')
    $Campos["__RequestVerificationToken"] = $m.Groups[1].Value
    $cuerpo = ($Campos.GetEnumerator() | ForEach-Object { [uri]::EscapeDataString($_.Key) + "=" + [uri]::EscapeDataString([string]$_.Value) }) -join "&"
    return Pedir POST $RutaPost -Formulario $cuerpo -Sesion $sesion
}

function Limpiar {
    Pedir DELETE "/api/telefonos/3001112233" | Out-Null
    Pedir DELETE "/api/telefonos/3001112244" | Out-Null
    Pedir DELETE "/api/personas/1999999901" | Out-Null
    Pedir DELETE "/api/personas/1999999902" | Out-Null
    Pedir DELETE "/api/profesiones/900" | Out-Null
}

Write-Output "Probando $BaseUrl  ($(Get-Date -Format 'yyyy-MM-dd HH:mm'))"
Write-Output ""
Limpiar

# ---------------------------------------------------------------- Swagger
Write-Output "== Swagger / OpenAPI =="
$r = Pedir GET "/swagger/v1/swagger.json"
Esperar "El documento OpenAPI responde" $r 200
Revisar "La especificación es OpenAPI 3" ($r.Cuerpo -match '"openapi"\s*:\s*"3\.') $r.Cuerpo.Substring(0, [Math]::Min(80, $r.Cuerpo.Length))
foreach ($ruta in "/api/personas", "/api/profesiones", "/api/estudios", "/api/telefonos") {
    Revisar "Swagger documenta $ruta" ($r.Cuerpo -match [regex]::Escape($ruta)) "no aparece en swagger.json"
}
Revisar "Swagger no expone los controladores de vistas" (-not ($r.Cuerpo -match '"/Personas')) "aparecen rutas MVC"
Esperar "La interfaz de Swagger UI carga" (Pedir GET "/swagger/index.html") 200

# ---------------------------------------------------------------- Personas
Write-Output ""
Write-Output "== API personas =="
$r = Pedir GET "/api/personas"
Esperar "Listar personas" $r 200
Revisar "Vienen los datos de ejemplo del DML (5 o más)" ((Contar $r.Cuerpo) -ge 5) $r.Cuerpo
Esperar "Consultar persona existente" (Pedir GET "/api/personas/1010101010") 200
Esperar "Consultar persona inexistente da 404" (Pedir GET "/api/personas/999") 404
$nueva = @{ cc = 1999999901; nombre = "Prueba"; apellido = "Humo"; genero = "F"; edad = 30 }
$r = Pedir POST "/api/personas" -Json $nueva
Esperar "Crear persona" $r 201
Revisar "Crear devuelve la ubicación del recurso" ($r.Cuerpo -match '1999999901') $r.Cuerpo
Esperar "Crear persona duplicada da 409" (Pedir POST "/api/personas" -Json $nueva) 409
Esperar "Género inválido da 400" (Pedir POST "/api/personas" -Json @{ cc = 1999999902; nombre = "X"; apellido = "Y"; genero = "X" }) 400
Esperar "Nombre vacío da 400" (Pedir POST "/api/personas" -Json @{ cc = 1999999902; nombre = ""; apellido = "Y"; genero = "M" }) 400
$nueva.edad = 31
Esperar "Actualizar persona" (Pedir PUT "/api/personas/1999999901" -Json $nueva) 204
Revisar "La actualización quedó guardada" (((Pedir GET "/api/personas/1999999901").Cuerpo | ConvertFrom-Json).edad -eq 31) "la edad no cambió"
Esperar "Cédula distinta en URL y cuerpo da 400" (Pedir PUT "/api/personas/1010101010" -Json $nueva) 400
Esperar "Actualizar persona inexistente da 404" (Pedir PUT "/api/personas/999" -Json @{ cc = 999; nombre = "A"; apellido = "B"; genero = "M" }) 404
Esperar "Teléfonos de una persona" (Pedir GET "/api/personas/1010101010/telefonos") 200
Esperar "Estudios de una persona" (Pedir GET "/api/personas/1010101010/estudios") 200

# ---------------------------------------------------------------- Profesiones
Write-Output ""
Write-Output "== API profesiones =="
Esperar "Listar profesiones" (Pedir GET "/api/profesiones") 200
$prof = @{ id = 900; nom = "Profesion de prueba"; des = "Creada por la prueba de humo" }
Esperar "Crear profesión" (Pedir POST "/api/profesiones" -Json $prof) 201
Esperar "Crear profesión duplicada da 409" (Pedir POST "/api/profesiones" -Json $prof) 409
Esperar "Consultar profesión" (Pedir GET "/api/profesiones/900") 200
$prof.nom = "Profesion de prueba editada"
Esperar "Actualizar profesión" (Pedir PUT "/api/profesiones/900" -Json $prof) 204
Esperar "Consultar profesión inexistente da 404" (Pedir GET "/api/profesiones/12345") 404

# ---------------------------------------------------------------- Estudios
Write-Output ""
Write-Output "== API estudios (llave compuesta) =="
Esperar "Listar estudios" (Pedir GET "/api/estudios") 200
$est = @{ idProf = 900; ccPer = 1999999901; fecha = "2020-12-01"; univer = "Universidad de Prueba" }
Esperar "Crear estudio" (Pedir POST "/api/estudios" -Json $est) 201
Esperar "Crear estudio duplicado da 409" (Pedir POST "/api/estudios" -Json $est) 409
Esperar "Estudio con persona inexistente da 400" (Pedir POST "/api/estudios" -Json @{ idProf = 900; ccPer = 999 }) 400
Esperar "Estudio con profesión inexistente da 400" (Pedir POST "/api/estudios" -Json @{ idProf = 12345; ccPer = 1999999901 }) 400
Esperar "Consultar estudio por llave compuesta" (Pedir GET "/api/estudios/900/1999999901") 200
$est.univer = "Universidad Editada"
Esperar "Actualizar estudio" (Pedir PUT "/api/estudios/900/1999999901" -Json $est) 204
Revisar "La universidad quedó actualizada" (((Pedir GET "/api/estudios/900/1999999901").Cuerpo | ConvertFrom-Json).univer -eq "Universidad Editada") "no cambió"

# ---------------------------------------------------------------- Teléfonos
Write-Output ""
Write-Output "== API teléfonos =="
Esperar "Listar teléfonos" (Pedir GET "/api/telefonos") 200
$tel = @{ num = "3001112233"; oper = "Claro"; duenio = 1999999901 }
Esperar "Crear teléfono" (Pedir POST "/api/telefonos" -Json $tel) 201
Esperar "Crear teléfono duplicado da 409" (Pedir POST "/api/telefonos" -Json $tel) 409
Esperar "Teléfono con dueño inexistente da 400" (Pedir POST "/api/telefonos" -Json @{ num = "3001112299"; oper = "Tigo"; duenio = 999 }) 400
Esperar "Número con letras da 400" (Pedir POST "/api/telefonos" -Json @{ num = "abc"; oper = "Tigo"; duenio = 1999999901 }) 400
$tel.oper = "Movistar"
Esperar "Actualizar teléfono" (Pedir PUT "/api/telefonos/3001112233" -Json $tel) 204
Revisar "El operador quedó actualizado" (((Pedir GET "/api/telefonos/3001112233").Cuerpo | ConvertFrom-Json).oper -eq "Movistar") "no cambió"
Revisar "El teléfono aparece en /api/personas/{cc}/telefonos" ((Pedir GET "/api/personas/1999999901/telefonos").Cuerpo -match "3001112233") "no aparece"

# ---------------------------------------------------------------- Vistas MVC
Write-Output ""
Write-Output "== Vistas web (MVC) =="
$r = Pedir GET "/"
Esperar "Página de inicio" $r 200
Revisar "Inicio muestra conexión a la base de datos" (-not ($r.Cuerpo -match "No hay conexi")) "dice que no hay conexión"
foreach ($ruta in "/Personas", "/Profesiones", "/Estudios", "/Telefonos",
                  "/Personas/Create", "/Profesiones/Create", "/Estudios/Create", "/Telefonos/Create",
                  "/Personas/Details/1999999901", "/Personas/Edit/1999999901", "/Personas/Delete/1999999901",
                  "/Profesiones/Details/900", "/Profesiones/Edit/900", "/Profesiones/Delete/900",
                  "/Estudios/Details?idProf=900&ccPer=1999999901", "/Estudios/Edit?idProf=900&ccPer=1999999901", "/Estudios/Delete?idProf=900&ccPer=1999999901",
                  "/Telefonos/Details/3001112233", "/Telefonos/Edit/3001112233", "/Telefonos/Delete/3001112233") {
    Esperar "GET $ruta" (Pedir GET $ruta) 200
}
Esperar "Detalle de persona inexistente da 404" (Pedir GET "/Personas/Details/999") 404
Revisar "La lista de personas muestra los datos" ((Pedir GET "/Personas").Cuerpo -match "Martinez") "no aparece Ana Martinez"

$r = EnviarFormulario "/Personas/Create" "/Personas/Create" @{ Cc = "1999999902"; Nombre = "Formulario"; Apellido = "Web"; Genero = "M"; Edad = "40" }
Esperar "Formulario: crear persona redirige a la lista" $r 302
Esperar "Formulario: la persona quedó en la base" (Pedir GET "/api/personas/1999999902") 200
$r = EnviarFormulario "/Personas/Create" "/Personas/Create" @{ Cc = "1999999902"; Nombre = "Repetida"; Apellido = "Web"; Genero = "M" }
Revisar "Formulario: cédula repetida muestra el error en la página" (($r.Codigo -eq 200) -and ($r.Cuerpo -match "Ya existe una persona")) ("código " + $r.Codigo)
$r = EnviarFormulario "/Personas/Create" "/Personas/Create" @{ Cc = "1999999903"; Nombre = ""; Apellido = "Web"; Genero = "Z" }
Revisar "Formulario: datos inválidos muestran mensajes de validación" (($r.Codigo -eq 200) -and ($r.Cuerpo -match "obligatorio")) ("código " + $r.Codigo)
$r = EnviarFormulario "/Personas/Edit/1999999902" "/Personas/Edit/1999999902" @{ Cc = "1999999902"; Nombre = "Formulario"; Apellido = "Editado"; Genero = "M"; Edad = "41" }
Esperar "Formulario: editar persona" $r 302
Revisar "Formulario: la edición quedó guardada" (((Pedir GET "/api/personas/1999999902").Cuerpo | ConvertFrom-Json).apellido -eq "Editado") "no cambió"
$r = EnviarFormulario "/Telefonos/Create" "/Telefonos/Create" @{ Num = "3001112244"; Oper = "Tigo"; Duenio = "1999999902" }
Esperar "Formulario: crear teléfono" $r 302
$r = EnviarFormulario "/Estudios/Create" "/Estudios/Create" @{ IdProf = "900"; CcPer = "1999999902"; Fecha = "2021-06-15"; Univer = "Universidad Web" }
Esperar "Formulario: crear estudio" $r 302
$r = EnviarFormulario "/Estudios/Edit?idProf=900&ccPer=1999999902" "/Estudios/Edit" @{ IdProf = "900"; CcPer = "1999999902"; Fecha = "2021-06-16"; Univer = "Universidad Web Editada" }
Esperar "Formulario: editar estudio" $r 302
$r = EnviarFormulario "/Profesiones/Edit/900" "/Profesiones/Edit/900" @{ Id = "900"; Nom = "Profesion editada por formulario"; Des = "ok" }
Esperar "Formulario: editar profesión" $r 302

# Borrar la persona por formulario debe borrar en cascada su teléfono y su estudio
$r = EnviarFormulario "/Personas/Delete/1999999902" "/Personas/Delete/1999999902" @{}
Esperar "Formulario: eliminar persona" $r 302
Esperar "Cascada: el teléfono de la persona eliminada ya no existe" (Pedir GET "/api/telefonos/3001112244") 404
Esperar "Cascada: el estudio de la persona eliminada ya no existe" (Pedir GET "/api/estudios/900/1999999902") 404

# ---------------------------------------------------------------- Borrados por API
Write-Output ""
Write-Output "== Eliminaciones por API =="
Esperar "Eliminar teléfono" (Pedir DELETE "/api/telefonos/3001112233") 204
Esperar "Eliminar estudio" (Pedir DELETE "/api/estudios/900/1999999901") 204
Esperar "Eliminar profesión" (Pedir DELETE "/api/profesiones/900") 204
Esperar "Eliminar persona" (Pedir DELETE "/api/personas/1999999901") 204
Esperar "Persona eliminada ya no existe" (Pedir GET "/api/personas/1999999901") 404
Esperar "Eliminar algo inexistente da 404" (Pedir DELETE "/api/personas/1999999901") 404
Revisar "Los datos de ejemplo siguen intactos" ((Contar (Pedir GET "/api/personas").Cuerpo) -ge 5) "se perdieron datos"

Limpiar
Write-Output ""
Write-Output ("RESULTADO: {0} pruebas OK, {1} fallidas" -f $script:ok, $script:fallos)
if ($script:fallos -gt 0) { exit 1 } else { exit 0 }
