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
  AsherLink3DConfig: function(str)
  {
	AsherLink3DConfig(Pointer_stringify(str));
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
   HelloFloat: function()
     {
     var userAgentInfo = navigator.userAgent;      
        var Agents = ["Android", "iPhone", "SymbianOS", "Windows Phone", "iPad", "iPod"];    
        var flag = true;       
        for (var v = 0; v < Agents.length; v++)
        {        
         if (userAgentInfo.indexOf(Agents[v]) > 0)
         {   
          return 1; //如果是手机端就返回1
           break;       
         }     
        }       
        return 2; //如果是pc就返回2
    },
 });
