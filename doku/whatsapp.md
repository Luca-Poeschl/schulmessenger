# Schulinternes _WhatsApp_

## Mindestanforderungen
* Die Anmeldung erfolgt mit Username und Passwort per LDAP.
* Es gibt als Rollen Schüler und Lehrer (Mitarbeiter).
* Sowohl Schüler als auch Lehrer können 1-zu-1 Chats erstellen.
* Man kann Text sowie Bilder und Dateien versenden.
* Erstellen von Gruppen mit beliebiger Zuordnung.
* Automatisches Erstellen von Gruppen auf Basis von Klassen.
* Reconnect-Logik: Nachrichten bei Verbindungsabbruch lokal zwischenspeichern und später versenden.

## Technologie
* Backend
  * C# mit ASP.NET, SignalR und Authentifizierung per LDAP.
  * Datenbank: MariaDB oder MS-SQL.
* Frontend
  * Z.B. Qt/QML als Multiplattform-App, 
  * oder C#/WPF mit Integration in KlaTab,
  * oder React.

## Erweiterungen
* Newsfeed, Erstellung von Posts durch Mitarbeiter.
* Frontend mit HTML, CSS, JavaScript und ggf. Integration in _Schulmanager_ aus der 11. Klasse.
* Je nach Frontend-Technologie eine App veröffentlichen.

## Besprechung(en)

### xx.9.2026
 *