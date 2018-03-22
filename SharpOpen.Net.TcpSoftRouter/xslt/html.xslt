<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="html" indent="yes"/>

  <xsl:template match="response">

    <html>
      <head>
        <title>SharpOpen.Net.TcpSoftRouter Administration</title>
        <meta http-equiv="content-type" content="text/html; charset=UTF-8" />
        <style type="text/css">
          .font { font-family: verdana,tahoma,arial; font-size:11px }
          th { height:30px; }
          td { font-size:11px; height:30px; }
          .header { background-color:#000; color:White; font-weight:bold; width:100%; height:30px; text-indent:10px; padding-top:10px; }
          .maintable { width:100%; }
          .maintable thead th { background-color:#ccc; color:#000; width:14%; text-align:left; }
          .maintable td { background-color:#fff; color:#000; width:14%; border-bottom: 1px solid #aaa; }
          .maintable td td { border-bottom: 0px; }
          .maintable input { border:0px; border-bottom:1px solid #ccc; width:100%; background-color:#ccc; }
          .maintable select { border:0px; border-top:1px solid #ccc; border-bottom:1px solid #ccc; width:100%; background-color:#ccc; }
          a { text-decoration:none; color:#f00; }
          a:hover { text-decoration:underline; }
        </style>
        <script language="javascript" type="text/javascript">
          var $ = document.getElementById;
          function postback(command)
          {
          if (command.indexOf("REMOVE ") == 0)
          {
          if (!confirm("Are you sure you wan't to remove the server ?")) return;
          }
          $("postbackcommand").value = command;
          document.forms[0].submit();
          }
          function postbackadd()
          {
          switch($("servertype").value)
          {
          case "1":
          $("postbackcommand").value = "ADD " + $("localport").value + " " + $("remotehost").value + " " + $("remoteport").value;
          break;
          case "2":
          $("postbackcommand").value = "ADD " + $("localport").value + " SOCKS " +      " " + $("sockshost").value + " " + $("socksport").value + " " + $("remotehost").value + " " + $("remoteport").value;
          break;
          case "3":
          $("postbackcommand").value = "SOCKS " + $("localport").value;
          break;
          }
          document.forms[0].submit();
          }
          function traceswitch(localport)
          {
          $("postbackcommand").value = "TRACE " + localport + " " + ($("traceswitch_" + localport).checked ? "ENABLE" : "DISABLE");
          document.forms[0].submit();
          }
        </script>
      </head>
      <body class="font">
        <form method="post" action="">
          <input type="hidden" name="postbackcommand" id="postbackcommand" />
          <div class="header">
            SharpOpen.Net.TcpSoftRouter Administration
          </div>

          <xsl:choose>
            <xsl:when test="servers">
              <table cellpadding="2" cellspacing="2" border="0" class="maintable font">
                <thead>
                  <tr>
                    <th>Local Port</th>
                    <th>Server Type</th>
                    <th>Remote Host</th>
                    <th>Socks Server</th>
                    <th>Status</th>
                    <th>Tracing</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  <xsl:for-each select="servers/server">
                    <tr>
                      <td>
                        <xsl:value-of select="@localport"/>
                      </td>
                      <td>
                        <xsl:choose>
                          <xsl:when test="@servertype='1'">Redirect</xsl:when>
                          <xsl:when test="@servertype='2'">Redirect Through Socks</xsl:when>
                          <xsl:when test="@servertype='3'">Socks</xsl:when>
                        </xsl:choose>
                      </td>
                      <td>
                        <xsl:choose>
                          <xsl:when test="@remotehost">
                            <xsl:value-of select="@remotehost"/>:<xsl:value-of select="@remoteport"/>
                          </xsl:when>
                          <xsl:otherwise>
                            <font color="#c0c0c0">N/A</font>
                          </xsl:otherwise>
                        </xsl:choose>
                      </td>
                      <td>
                        <xsl:choose>
                          <xsl:when test="@servertype='2'">
                            <xsl:value-of select="@sockshost"/>:<xsl:value-of select="@socksport"/>
                          </xsl:when>
                          <xsl:otherwise>
                            <font color="#c0c0c0">N/A</font>
                          </xsl:otherwise>
                        </xsl:choose>
                      </td>
                      <td>
                        <xsl:choose>
                          <xsl:when test="@isrunning='1'">Started</xsl:when>
                          <xsl:when test="@isrunning='0'">Stopped</xsl:when>
                        </xsl:choose>
                      </td>
                      <td>
                        <input style="background-color:white;border:0px;" type="checkbox" id="traceswitch_{@localport}" onclick="javascript:traceswitch({@localport});"></input>
                        <script language="javascript" type="text/javascript">
                          if(<xsl:value-of select="@isbeingtraced"/> == 1) $('traceswitch_<xsl:value-of select="@localport"/>').checked = "checked";
                        </script>
                      </td>
                      <td>
                        <table cellspacing="0" cellpadding="0">
                          <tr>
                            <td>
                              <a href="javascript:postback('START {@localport}')">Start</a>
                            </td>
                            <td>
                              <a href="javascript:postback('STOP {@localport}')">Stop</a>
                            </td>
                            <td>
                              <a href="javascript:postback('TEST {@localport}')">Test</a>
                            </td>
                            <td>
                              <a href="javascript:postback('REMOVE {@localport}')">Remove</a>
                            </td>
                          </tr>
                        </table>
                      </td>
                    </tr>
                  </xsl:for-each>
                  <tr>
                    <td>
                      <input class="font" type="text" name="localport" id="localport" maxlength="5" />
                    </td>
                    <td>
                      <select class="font" id="servertype" name="type">
                        <option value="1">Redirect</option>
                        <option value="2">Redirect Through Socks</option>
                        <option value="3">Socks</option>
                      </select>
                    </td>
                    <td>
                      <input class="font" style="width:70%;" type="text" name="remotehost" id="remotehost" maxlength="32" /> : <input class="font" style="width:20%;" type="text" name="remoteport" id="remoteport" maxlength="5" />
                    </td>
                    <td>
                      <input class="font" style="width:70%;" type="text" name="sockshost" id="sockshost" maxlength="32" /> : <input class="font" style="width:20%;" type="text" name="socksport" id="socksport" maxlength="5" />
                    </td>
                    <td>
                      <font color="#c0c0c0">N/A</font>
                    </td>
                    <td>
                      <font color="#c0c0c0">N/A</font>
                    </td>
                    <td>
                      <table cellspacing="0" cellpadding="0">
                        <tr>
                          <td>
                            <a href="javascript:postbackadd();">Add</a>
                          </td>
                        </tr>
                      </table>
                    </td>
                  </tr>
                </tbody>
              </table>
            </xsl:when>
            <xsl:otherwise>
              <br/>
              <xsl:value-of select="@statusmessage"/>
              <br/>
              <br/>
              <a href="/">Back</a>
            </xsl:otherwise>
          </xsl:choose>
        </form>
      </body>
    </html>


  </xsl:template>

</xsl:stylesheet>
