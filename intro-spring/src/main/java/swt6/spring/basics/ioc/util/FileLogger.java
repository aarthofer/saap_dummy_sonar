package swt6.spring.basics.ioc.util;

import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.PrintWriter;


public class FileLogger {

  private PrintWriter writer;
  
  public FileLogger() {
    init("log.txt");
  }

  public FileLogger(String fileName) {
    init(fileName);
  }

  private void init(String fileName) {
    try {
      writer = new PrintWriter(new FileOutputStream(fileName));
    }
    catch (FileNotFoundException e) {
      e.printStackTrace();
    }
  }
  
  public void setFileName(String fileName) {
    init(fileName);
  }
  
  public void log(String msg) {
    writer.println("Log: " + msg);
    writer.flush();
  }

  public void close() {
    writer.close();
  }
}
