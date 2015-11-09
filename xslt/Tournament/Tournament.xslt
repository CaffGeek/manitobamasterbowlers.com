<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
  <xsl:output method="xml" indent="yes"/>

  <xsl:param name="fileName" />
  <xsl:param name="tournamentId" />

  <xsl:template match="/">
    <TournamentResults status="">

      <!-- First, validate the input -->
      <Errors>
        <!--Not sure what's needed here-->
      </Errors>

      <!-- Then unify the XML -->
      <xsl:for-each select="//TournamentResult">
        <TournamentResult status="failure" fileName="{$fileName}" tournamentId="{$tournamentId}">
          <xsl:copy-of select="child::*"/>
        </TournamentResult>
      </xsl:for-each>
    </TournamentResults>
  </xsl:template>
</xsl:stylesheet>
