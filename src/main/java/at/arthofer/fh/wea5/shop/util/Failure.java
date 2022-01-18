package at.arthofer.fh.wea5.shop.util;

import java.io.PrintWriter;
import java.io.Serializable;
import java.io.StringWriter;
import java.sql.SQLException;

import javax.enterprise.context.RequestScoped;
import javax.inject.Named;
import javax.servlet.ServletException;

/*
 * stores failure messages
 */ 

@Named("failureModel")
@RequestScoped
public class Failure implements Serializable {
	private static final long serialVersionUID = 1L;
	private Exception exception = new Exception("Dummy exception");
	private String message = new String("An exception occured!");

	public void setException(Exception e) {
		this.exception = e;
	}

	public Exception getException() {
		return this.exception;
	}

	public void setMessage(String msg) {
		this.message = msg;
	}

	public String getMessage() {
		return this.message;
	}

	public String getStackTrace() {
		StringWriter sw = new StringWriter();
		PrintWriter pw = new PrintWriter(sw);
		fillStackTrace(this.getException(), pw);
		return sw.toString();
	}

	private static void fillStackTrace(Throwable t, PrintWriter w) {
		if (t == null) {
			return;
		}
		t.printStackTrace(w);
		if (t instanceof ServletException) {
			Throwable cause = ((ServletException) t).getRootCause();
			if (cause != null) {
				w.println("Root cause:");
				fillStackTrace(cause, w);
			}
		} else if (t instanceof SQLException) {
			Throwable cause = ((SQLException) t).getNextException();
			if (cause != null) {
				w.println("Next exception:");
				fillStackTrace(cause, w);
			}
		} else {
			Throwable cause = t.getCause();
			if (cause != null) {
				w.println("Cause:");
				fillStackTrace(cause, w);
			}
		}
	}
}
