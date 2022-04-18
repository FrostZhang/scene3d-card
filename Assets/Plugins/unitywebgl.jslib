mergeInto(LibraryManager.library, 
{
 
  AsherLink3DStart: function ()
  {
    AsherLink3DStart();
  },
  
  AsherLink3DClickMessage: function(str)
  {
	 AsherLink3DClickMessage(Pointer_stringify(str));
  },
  AsherLink3DLongClickMessage: function(str)
  {
	 AsherLink3DLongClickMessage(Pointer_stringify(str));
  },
  HelloString: function (str) 
  {
    window.alert(Pointer_stringify(str));
  },
 AsherLink3DWebLog: function(str)
 {
     AsherLink3DWebLog(Pointer_stringify(str));
 },
   HelloFloat: function () 
   {
       return 1;
   },
 
 });
