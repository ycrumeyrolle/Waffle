using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Waffle")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyProduct("Waffle")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("e97cfc27-22c5-438a-8b31-0abf59655975")]
[assembly: InternalsVisibleTo("Waffle.Tests")]
[assembly: InternalsVisibleTo("Waffle.Unity")]
[assembly: InternalsVisibleTo("Waffle.Events.MongoDb")]
[assembly: InternalsVisibleTo("Waffle.Queries.Data")]
[assembly: InternalsVisibleTo("Waffle.Queuing.MongoDb")]