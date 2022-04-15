mergeInto(LibraryManager.library, 
{
 
  UnityStart: function ()
  {
    UnityStart();
  },
  
  LightMessage: function(str)
  {
	 LightMessage(Pointer_stringify(str));
  },
  HelloString: function (str) 
  {
    window.alert(Pointer_stringify(str));
  },
 
   HelloFloat: function () 
   {
       return 1;
   },
 
 });
