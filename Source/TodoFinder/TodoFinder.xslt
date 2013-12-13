<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl">
  <xsl:output method="html" indent="yes"/>

  <xsl:template match="/">
    <html>
      <head>
        <style>
          body { font-family: Helvetica, Arial }
          .file .header { font-weight: bold; }
          .linenumber { color: gray; border: solid 1px lightgray; padding: 0 0.2em; }
        </style>
      </head>
      <body>
        <h1>TODO report</h1>
        <xsl:apply-templates/>
      </body>
    </html>
  </xsl:template>

  <xsl:template match="File">
    <div class="file">
      <div class="header">
        <xsl:value-of select="@Path"/>
      </div>
      <ul>
        <xsl:apply-templates/>
      </ul>
    </div>
  </xsl:template>

  <xsl:template match="Comment">
    <li class="comment">
      <xsl:value-of select="."/> <span class="linenumber">line <xsl:value-of select="@Line"/></span>
    </li>
  </xsl:template>

</xsl:stylesheet>
