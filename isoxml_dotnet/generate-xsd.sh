#TASKDATA
rm -rf ./resources/xsd_converted
mkdir resources/xsd_converted
xsltproc resources/xslt/addAnnotations.xsl resources/xsd_original/ISO11783_Common_V4-3.xsd > resources/xsd_converted/ISO11783_Common_V4-3.xsd
xsltproc resources/xslt/addAnnotations.xsl resources/xsd_original/ISO11783_TaskFile_V4-3.xsd > resources/xsd_converted/ISO11783_TaskFile_V4-3.xsd
xsltproc resources/xslt/addAnnotations.xsl resources/xsd_original/ISO11783_ExternalFile_V4-3.xsd > resources/xsd_converted/ISO11783_ExternalFile_V4-3.xsd
xsltproc resources/xslt/addAnnotations.xsl resources/xsd_original/ISO11783_TimeLog_V4-3.xsd > resources/xsd_converted/ISO11783_TimeLog_V4-3.xsd
xsltproc resources/xslt/addAnnotations.xsl resources/xsd_original/ISO11783_LinkListFile_V4-3.xsd > resources/xsd_converted/ISO11783_LinkListFile_V4-3.xsd

rm -rf ./src/xml/

mkdir ./src/xml/
xsd ./resources/xsd_converted/ISO11783_Common_V4-3.xsd  -c -namespace:de.dev4agriculture.iso11783.common -out:src/xml
xsd ./resources/xsd_converted/ISO11783_TaskFile_V4-3.xsd -c -namespace:de.dev4agriculture.iso11783.taskdata -out:src/xml
xsd ./resources/xsd_converted/ISO11783_ExternalFile_V4-3.xsd -c -namespace:de.dev4agriculture.iso11783.external -out:src/xml
xsd ./resources/xsd_converted/ISO11783_TimeLog_V4-3.xsd -c -namespace:de.dev4agriculture.iso11783.timelog -out:src/xml
xsd ./resources/xsd_converted/ISO11783_LinkListFile_V4-3.xsd -c -namespace:de.dev4agriculture.iso11783.linklist -out:src/xml

mv ./src/xml/ISO11783_Common_V4-3.cs ./src/xml/ISO11783_Common.cs
mv ./src/xml/ISO11783_TaskFile_V4-3.cs ./src/xml/ISO11783_TaskFile.cs
mv ./src/xml/ISO11783_ExternalFile_V4-3.cs ./src/xml/ISO11783_ExternalFile.cs
mv ./src/xml/ISO11783_TimeLog_V4-3.cs ./src/xml/ISO11783_TimeLog.cs
mv ./src/xml/ISO11783_LinkListFile_V4-3.cs ./src/xml/ISO11783_LinkListFile.cs

