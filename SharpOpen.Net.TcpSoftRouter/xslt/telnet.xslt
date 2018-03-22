<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="text" indent="no" />

  <xsl:template match="response">
    
    <xsl:value-of select="@statuscode"/><xsl:text> </xsl:text><xsl:value-of select="@statusmessage"/>

    <xsl:call-template name="NewLine"/>
    
    <xsl:for-each select="servers">
      <xsl:call-template name="NewLine"/>
      <xsl:text>NUMBER OF SERVERS: </xsl:text><xsl:value-of select="@count"/>
      <xsl:call-template name="NewLine"/>
    </xsl:for-each>

    <xsl:for-each select="servers/server">
      
      <xsl:call-template name="NewLine"/>

      <xsl:value-of select="@localport"/>
      <xsl:text> </xsl:text>
      
      <xsl:choose>
        <xsl:when test="@isrunning='1'">(STARTED) </xsl:when>
        <xsl:when test="@isrunning='0'">(STOPPED) </xsl:when>
      </xsl:choose>
      
      <xsl:choose>
        <xsl:when test="@servertype='1'">Redirect To <xsl:value-of select="@remotehost"/>:<xsl:value-of select="@remoteport"/></xsl:when>
        <xsl:when test="@servertype='2'">Redirect To <xsl:value-of select="@remotehost"/>:<xsl:value-of select="@remoteport"/> (Socks <xsl:value-of select="@sockshost"/>:<xsl:value-of select="@socksport"/>)</xsl:when>
        <xsl:when test="@servertype='3'">Socks Server</xsl:when>
      </xsl:choose>
      
    </xsl:for-each>
    <xsl:call-template name="NewLine"/>

  </xsl:template>

  <xsl:template name="NewLine">
<xsl:text>
</xsl:text>
  </xsl:template>
  
</xsl:stylesheet>