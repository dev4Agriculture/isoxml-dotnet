<?xml version="1.0"?>
<xsl:stylesheet
	xmlns:xs="http://www.w3.org/2001/XMLSchema"
	xmlns:jaxb="http://java.sun.com/xml/ns/jaxb" version="1.0"
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:strip-space elements="*" />
	<xsl:output method="xml" indent="yes" />
	<xsl:template match="@*|node()">
		<xsl:copy>
			<xsl:apply-templates select="@*|node()" />
		</xsl:copy>
	</xsl:template>

	<xsl:template match="/*">
		<xsl:copy>
			<xsl:attribute name="jaxb:version">2.1</xsl:attribute>
			<xsl:apply-templates select="@*|node()" />
		</xsl:copy>
	</xsl:template>

	<xsl:template
		match="*[local-name()!='choice']/xs:element/xs:annotation">
		<xs:annotation>
			<xs:appinfo>
				<xsl:for-each select="xs:documentation">
					<jaxb:class>
						<xsl:attribute name="name">
                            <xsl:value-of
							select="normalize-space(translate(text(), ' ', ''))" />
                        </xsl:attribute>
					</jaxb:class>
				</xsl:for-each>
			</xs:appinfo>
		</xs:annotation>
	</xsl:template>
	<!-- Remove attributes from choice -->
	<xsl:template match="xs:choice">
		<xs:choice>
			<xsl:apply-templates select="node()" />
		</xs:choice>
	</xsl:template>
	<xsl:template match="xs:choice/xs:element/xs:annotation">
		<xs:annotation>
			<xs:appinfo>
				<xsl:for-each select="xs:documentation">
					<jaxb:property>
						<xsl:attribute name="name">
                            <xsl:value-of
							select="normalize-space(translate(translate(text(), ' ', '_'), ',', ''))" />
                        </xsl:attribute>
					</jaxb:property>
				</xsl:for-each>
			</xs:appinfo>
		</xs:annotation>
	</xsl:template>

	<xsl:template match="xs:attribute/xs:annotation">
		<xs:annotation>
			<xs:appinfo>
				<xsl:for-each select="xs:documentation">
					<jaxb:property>
						<xsl:attribute name="name">
                            <xsl:value-of
							select="normalize-space(text())" />
                        </xsl:attribute>
					</jaxb:property>
				</xsl:for-each>
			</xs:appinfo>
		</xs:annotation>
	</xsl:template>
	<xsl:template
		match="*[local-name() != 'union' and string-length(@name) = 1]/xs:simpleType[count(xs:restriction/xs:enumeration) > 0]">
		<xs:simpleType>
			<xs:annotation>
				<xs:appinfo>
					<jaxb:typesafeEnumClass>
						<xsl:variable name="typeName">
							<xsl:choose>
								<xsl:when
									test="translate(normalize-space(parent::xs:attribute/xs:annotation/xs:documentation), ' ', '_') != ''">
									<xsl:value-of
										select="translate(normalize-space(parent::xs:attribute/xs:annotation/xs:documentation), ' ', '_')" />
								</xsl:when>
								<xsl:otherwise>
									<xsl:value-of
										select="translate(normalize-space(parent::xs:attribute/parent::xs:complexType/parent::xs:element/xs:annotation/xs:documentation), ' ', '_')" />
								</xsl:otherwise>
							</xsl:choose>
						</xsl:variable>
						<xsl:variable name="typeClass">
							<xsl:value-of select="normalize-space(../../../xs:annotation/xs:documentation/text())"></xsl:value-of>
						</xsl:variable>
						<xsl:attribute name="name"><xsl:if test="$typeName = 'Type'"><xsl:value-of select="$typeClass"/></xsl:if><xsl:value-of
							select="$typeName" /></xsl:attribute>
					</jaxb:typesafeEnumClass>
				</xs:appinfo>
			</xs:annotation>
			<xsl:apply-templates
				select="@*|*[local-name()!='annotation']" />
		</xs:simpleType>
	</xsl:template>

	<!-- Process all enumerations that have numbers -->
	<xsl:template match="xs:enumeration">
		<xs:enumeration>
			<xsl:attribute name="value">
				<xsl:value-of select="@value"></xsl:value-of>
			</xsl:attribute>
			<xs:annotation>
				<xs:appinfo>
					<jaxb:typesafeEnumMember>
						<xsl:attribute name="name">
							<xsl:choose>
								<xsl:when
							test="count(xs:annotation/xs:documentation) > 0"><xsl:value-of
							select="translate(normalize-space(xs:annotation/xs:documentation),'abcdefghijklmnopqrstuvwxyz ,().+-','ABCDEFGHIJKLMNOPQRSTUVWXYZ_')" /><xsl:if
							test="normalize-space(xs:annotation/xs:documentation) = 'Reserved'">_<xsl:value-of select="@value" /></xsl:if></xsl:when>
								<xsl:when test="@value = 0">ZERO</xsl:when>
								<xsl:when test="@value = 1">ONE</xsl:when>
								<xsl:when test="@value = 2">TWO</xsl:when>
								<xsl:when test="@value = 3">THREE</xsl:when>
								<xsl:when test="@value = 4">FOUR</xsl:when>
								<xsl:when test="@value = 5">FIVE</xsl:when>
								<xsl:when test="@value = 6">SIX</xsl:when>
								<xsl:when test="@value = 7">SEVEN</xsl:when>
								<xsl:when test="@value = 8">EIGHT</xsl:when>
								<xsl:when test="@value = 9">NINE</xsl:when>
							</xsl:choose>
						</xsl:attribute>
					</jaxb:typesafeEnumMember>
				</xs:appinfo>
			</xs:annotation>
		</xs:enumeration>
	</xsl:template>
</xsl:stylesheet>