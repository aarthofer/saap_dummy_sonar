package at.arthofer.fh.wea5.shop.warehouse;

import java.io.Serializable;

public class ArticleData implements Serializable {

	private static final long serialVersionUID = 6504554011687690670L;

	private java.lang.String id;
	private String isbn;
	private String author;
	private java.lang.String title;
	private java.lang.String publisher;
	private java.lang.String year;
	private java.lang.String price;

	public ArticleData(String id, String isbn, String author, String title, String publisher, String year,
			String price) {
		setId(id);
		setIsbn(isbn);
		setAuthor(author);
		setTitle(title);
		setPublisher(publisher);
		setYear(year);
		setPrice(price);
	}

	public java.lang.String getId() {
		return this.id;
	}

	public void setId(java.lang.String id) {
		this.id = id;
	}

	public String getPrice() {
		return this.price;
	}

	public void setPrice(String price) {
		this.price = price;
	}

	public String getIsbn() {
		return isbn;
	}

	public void setIsbn(String isbn) {
		this.isbn = isbn;
	}

	public java.lang.String getPublisher() {
		return publisher;
	}

	public void setPublisher(java.lang.String publisher) {
		this.publisher = publisher;
	}

	public java.lang.String getTitle() {
		return title;
	}

	public void setTitle(java.lang.String title) {
		this.title = title;
	}

	public java.lang.String getYear() {
		return year;
	}

	public void setYear(java.lang.String year) {
		this.year = year;
	}

	public String getAuthor() {
		return author;
	}

	public void setAuthor(String author) {
		this.author = author;
	}

	public String toString() {
		return "id=" + getId() + " " + "title=" + getTitle();
	}

	public boolean equals(Object obj) {
		if (obj instanceof ArticleData) {
			ArticleData that = (ArticleData) obj;
			return this.getId().equals(that.getId());
		}
		return false;
	}

}
