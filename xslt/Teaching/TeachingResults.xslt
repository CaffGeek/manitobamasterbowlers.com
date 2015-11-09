<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl"
>
	<xsl:output method="xml" indent="yes"/>

  <xsl:variable name="lowercase">abcdefghijklmnopqrstuvwxyz</xsl:variable>
  <xsl:variable name="uppercase">ABCDEFGHIJKLMNOPQRSTUVWXYZ</xsl:variable>

	<xsl:template match="/">
		<TeachingResults>		
			<xsl:for-each select="//Table[position()>1 and .!= '']">
        <TeachingResult>
					<Bowler>
						<xsl:value-of select="F1"/>
					</Bowler>
					<Average>
						<xsl:value-of select="F2"/>
					</Average>
          <Game number="1">
            <xsl:value-of select="F3"/>
          </Game>
          <Game number="2">
            <xsl:value-of select="F4"/>
          </Game>
          <Game number="3">
            <xsl:value-of select="F5"/>
          </Game>
          <Game number="4">
            <xsl:value-of select="F6"/>
          </Game>
          <Game number="5">
            <xsl:value-of select="F7"/>
          </Game>
          <Game number="6">
            <xsl:value-of select="F8"/>
          </Game>
          <Game number="7">
            <xsl:value-of select="F9"/>
          </Game>
          <Game number="8">
            <xsl:value-of select="F10"/>
          </Game>
          <NewBowler>
            <xsl:if test="translate(substring(F11,1,1), $lowercase, $uppercase) = 'Y'">
              <xsl:text>1</xsl:text>
            </xsl:if>
            <xsl:if test="not(translate(substring(F11,1,1), $lowercase, $uppercase) = 'Y')">
              <xsl:text>0</xsl:text>
            </xsl:if>
          </NewBowler>
			</TeachingResult>
			</xsl:for-each>
		</TeachingResults>	
</xsl:template>

	<xsl:template name="value">
		<xsl:param name="n"/>
		<xsl:value-of select="translate(translate($n,'$',''),',','')"/>
		<xsl:if test="boolean($n)= false">0</xsl:if>
		<xsl:if test="$n = ''">0</xsl:if>
	</xsl:template>
</xsl:stylesheet>
