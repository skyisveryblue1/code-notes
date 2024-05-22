package com.test;

import org.bouncycastle.asn1.x500.X500Name;
import org.bouncycastle.asn1.x509.SubjectPublicKeyInfo;
import org.bouncycastle.pkcs.PKCS10CertificationRequest;
import org.bouncycastle.util.io.pem.PemObject;
import org.bouncycastle.util.io.pem.PemReader;

import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.IOException;

public class CsrDecoder {

	public static void main(String[] args) {
		// Path to your CSR file
        String csrFilePath = args[0]; //"D:/Rich.csr"

     // Read the CSR file using Bouncy Castle PEMReader
        try (PemReader pemReader = new PemReader(new FileReader(csrFilePath))) {
            PemObject pemObject = pemReader.readPemObject();
            PKCS10CertificationRequest csr = new PKCS10CertificationRequest(pemObject.getContent());

            // Access the CSR information
            X500Name subject = csr.getSubject();
            SubjectPublicKeyInfo publicKeyInfo = csr.getSubjectPublicKeyInfo();
            
            // Display the CSR information
            System.out.println("Subject Name: " + subject.toString());
            System.out.println("Public Key Algorithm: " + publicKeyInfo.getAlgorithm().getAlgorithm());
            System.out.println("Public Key Data: " + publicKeyInfo.getPublicKeyData());
        } catch (FileNotFoundException e) {
			e.printStackTrace();
		} catch (IOException e) {
			e.printStackTrace();
		}
	}

}
