$( document ).ready(function() {
    $(".ajaxq").submit(function(e) {
        e.preventDefault();
        var form = $(this);
        var url = form.attr('action');
        var method = form.attr('method');
        $.ajax({
               type: method,
               url: url,
               data: form.serialize(),
               success: function(data)
               {
                   try
                   {
                       jsondata = JSON.parse(data);
                       var redirectdelay = 1500;
                       if(jsondata.hasOwnProperty('status'))
                       {
                           if(jsondata.hasOwnProperty('message'))
                           {
                               if(jsondata.status == false)
                               {
                                   redirectdelay = 3500;
                                   if(jsondata.message != "") alert_warning(jsondata.message);
                               }
                           }
                           if(jsondata.hasOwnProperty('redirect'))
                           {
                               if(jsondata.redirect != "")
                               {
                                   if(jsondata.redirect != null)
                                   {
                                    setTimeout(function(){ $(location).attr('href', jsondata.redirect) }, redirectdelay);
                                    }
                               }
                           }
                       }
                       else
                       {
                           alert_error("Reply from server is not vaild please reload the page and try again.");
                       }
                   }
                   catch(e)
                   {
                       alert_warning("Unable to process reply, please reload the page and try again!");
                   }
               },
               error: function (data)
               {
                   alert_error(data);
               }
        });
    });
    $(".ajax").submit(function(e) {
        e.preventDefault();
        var form = $(this);
        var url = form.attr('action');
        var method = form.attr('method');
        $.ajax({
               type: method,
               url: url,
               data: form.serialize(),
               success: function(data)
               {
                   try
                   {
                       jsondata = JSON.parse(data);
                       var redirectdelay = 1500;
                       if(jsondata.hasOwnProperty('status'))
                       {
                           if(jsondata.hasOwnProperty('message'))
                           {
                               if(jsondata.status == true)
                               {
                                   if(jsondata.message != "") alert_success(jsondata.message);
                               }
                               else
                               {
                                   redirectdelay = 3500;
                                   if(jsondata.message != "") alert_warning(jsondata.message);
                               }
                           }
                           if(jsondata.hasOwnProperty('redirect'))
                           {
                               if(jsondata.redirect != "")
                               {
                                   if(jsondata.redirect != null)
                                   {
                                    setTimeout(function(){ $(location).attr('href', jsondata.redirect) }, redirectdelay);
                                    }
                               }
                           }
                       }
                       else
                       {
                           alert_error("Reply from server is not vaild please reload the page and try again.");
                       }
                   }
                   catch(e)
                   {
                       alert_warning("Unable to process reply, please reload the page and try again!");
                   }
               },
               error: function (data)
               {
                   alert_error(data);
               }
        });
    });
});

function alert_success(smsg)
{
    alert(smsg,"success");
}
function alert_error(smsg)
{
    alert(smsg,"error");
}
function alert_warning(smsg)
{
    alert(smsg,"warning");
}
function alert(smsg,alerttype)
{
    $.notify({
    	// options
    	message: smsg
    },{
    	// settings
    	type: alerttype
    });
}

function update_localchat(urlbase)
{
    $.ajax({
        type: "post",
        url: ""+urlbase+"ajax/localchat/get",
        success: function(data)
        {
            try
            {
                jsondata = JSON.parse(data);
                var redirectdelay = 1500;
                if(jsondata.hasOwnProperty('status'))
                {
                    if(jsondata.hasOwnProperty('message'))
                    {
                        if(jsondata.status == true)
                        {
                            $('#localchat').prop('readonly',false);
                            var text = "";
                            var addon = "";
                            $.each( JSON.parse(jsondata.message), function( key, value ) {
                              text = ""+value+""+addon+""+text+"";
                              addon = "\n";
                            });
                            $('#localchat').val(text);
                            $('#localchat').prop('readonly',true);
						}
                    }
                }
            }
            catch(e)
            {
                alert_warning("Unable to process reply, please reload the page and try again!");
            }
        }
    });
}
function update_groupchat(urlbase,groupuuid)
{
    $.ajax({
        type: "post",
        url: ""+urlbase+"ajax/groupchat/get",
        data: "groupuuid="+groupuuid+"",
        success: function(data)
        {
            try
            {
                jsondata = JSON.parse(data);
                var redirectdelay = 1500;
                if(jsondata.hasOwnProperty('status'))
                {
                    if(jsondata.hasOwnProperty('message'))
                    {
                        if(jsondata.status == true)
                        {
                            $("localchat").val(jsondata.message);
						}
                    }
                }
            }
            catch(e)
            {
                alert_warning("Unable to process reply, please reload the page and try again!");
            }
        }
    });
}